
using Newtonsoft.Json;
using JoltXServer.Models;
using JoltXServer.DataAccessLayer;
using JoltXServer.Repositories;
using System.Net.WebSockets;
using Microsoft.VisualBasic;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;
using SQLitePCL;
using System.Diagnostics;

namespace JoltXServer.Services;

public class BinanceService : IExternalAPIService
{
    private enum Priority : ushort
    {
        High = 1,
        Medium = 2,
        Low = 3
    };

    private struct ApiRequest
    {
        public ApiRequest(string symbol, long startTime, long endTime, bool isMostRecent)
        {
            Symbol = symbol;
            StartTime = startTime;
            EndTime = endTime;
            IsMostRecentCandles = isMostRecent;
        }

        public string Symbol { get; }
        public long StartTime { get; }
        public long EndTime { get; }
        public bool IsMostRecentCandles { get; }

        public override string ToString() => $"ApiRequest for symbol: {Symbol} start time: {StartTime} end time: {EndTime}";
    }

    private static readonly int MSECONDS_IN_HOUR = 3_600_000;
    private static readonly int MSECONDS_IN_MINUTE = 60_000;
    // TODO update this limit rate from API regularly
    private static int _binanceCandleLimitPerRequest = 1500;

    private static IDictionary<string, long> _activeSymbols;

    private static readonly string _binanceUrl = "https://api3.binance.com/api/v3";
    // `${klineEndpoint}?symbol=${s}&interval=${timeFrame}&limit=${API_KLINE_LIMIT}`
    // `&startTime=${startTime.toString()}&endTime=${endTime}
    private static string _binanceWebSocketUrl = "wss://stream.binance.com/stream?streams=";
    // wss://stream.binance.com:9443/stream?streams=ethbtc@kline_1m/linkusdt@kline_1m
    private static ClientWebSocket? _ws;

    private static PriorityQueue<ApiRequest, Priority> _apiQue;
    private static bool _queIsProcessing = false;

    private static bool _resetWebsocket = false;
    private readonly ISymbolRepository _symbolRepository;
    private readonly ICandleRepository _candleRepository;


    public BinanceService(ISymbolRepository symbolRepository, ICandleRepository candleRepository)
    {
        Console.WriteLine("Creating BinanceService object");
        _symbolRepository = symbolRepository;
        _candleRepository = candleRepository;

        _apiQue = new PriorityQueue<ApiRequest, Priority>();

        Startup();
    }

    private async void Startup()
    {
        await StartupActivateSymbols();
        await WebSocketLoop();
    }

    private async Task StartupActivateSymbols()
    {
        _activeSymbols = new Dictionary<string, long>();

        string[] symbolNames = await _symbolRepository.GetAllActiveSymbolNames();
        for(int i = 0; i < symbolNames.Length; i++)
        {
            await ActivateOneSymbol(symbolNames[i], true);
            Console.WriteLine($"Adding symbol {symbolNames[i]} to active list");
        }
    }

    // add symbol to ws url
    // update last candle time from db
    // restart websocket
    public async Task ActivateOneSymbol(string symbol, bool isStartup = false)
    {
        if(!_binanceWebSocketUrl.EndsWith('=')) _binanceWebSocketUrl += "/";        
        _binanceWebSocketUrl += $"{symbol.ToLower()}@kline_1m";

        // get last candle time from db
        long lastCandleTime = await _candleRepository.GetLastCandleTime(symbol+'m');
        _activeSymbols.Add(symbol, lastCandleTime);

        _resetWebsocket = !isStartup;
    }

    public async Task<List<Candle>?> GetCandlesAsync(string symbol, long startTime = 0, long endTime = 0)
    {
        string requestUrl = _binanceUrl + $"/klines?symbol={symbol}&interval=1m&limit={_binanceCandleLimitPerRequest}";
        if(startTime > 0) requestUrl += $"&startTime={startTime}";
        if(endTime > 0) requestUrl += $"&endTime={endTime}";

        HttpClient client = new();        
        HttpResponseMessage response = await client.GetAsync(requestUrl);

        if(!response.IsSuccessStatusCode)
            return null;

        var jsonResponse = await response.Content.ReadAsStringAsync();        

        List<Candle>? candleSticks = JsonConvert.DeserializeObject<List<Candle>>(jsonResponse);

        return candleSticks;
    }

    private JToken? parseBuffer(byte[] buffer, int size)
    {
        try
        {
            // convert byte buffer to string, and parse to get candle data
            string jsonData = Encoding.UTF8.GetString(buffer, 0, size);
            int index = jsonData.IndexOf("\"k\":") + 4;
            jsonData = jsonData.Substring(index, jsonData.Length - index - 2);

            // deserialise json string to JToken
            return JsonConvert.DeserializeObject<JToken>(jsonData);
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex);
            return null;
        }
    }

    private async Task WebSocketLoop()
    {
        while(true)
        {
            using(_ws = new ClientWebSocket())
            {
                Console.WriteLine($"Connecting to websocket {_binanceWebSocketUrl}");
                await _ws.ConnectAsync(new Uri(_binanceWebSocketUrl), CancellationToken.None);
                byte[] buffer = new byte[1024];
                while (_ws.State == WebSocketState.Open)
                {
                    var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close) break;

                    var candleData = parseBuffer(buffer, result.Count);

                    // if the websocket candle is closed, add the candle
                    if(candleData != null && (bool)candleData["x"])
                        addCandle(candleData);          

                    if(_resetWebsocket == true) break;
                }
                await closeWebSocket();
            }
        }
    }

    private async Task addCandle(JToken candleData)
    {
        string symbol = (string)candleData["s"];
        long lastCandleTime = _activeSymbols[(string)candleData["s"]];
        long currentCandleTime = (long)candleData["t"];

        Console.WriteLine($"Adding candle from websocket {symbol} last candle time: {lastCandleTime} current time: {currentCandleTime}");
        if(lastCandleTime == currentCandleTime - MSECONDS_IN_MINUTE || lastCandleTime == 0)
        {
            Candle newCandle = new()
            {
                Time = (long)candleData["t"],
                Open = (decimal)candleData["o"],
                High = (decimal)candleData["h"],
                Low = (decimal)candleData["l"],
                Close = (decimal)candleData["c"],
                Volume = (decimal)candleData["v"]
            };
            await _candleRepository.InsertOneCandle(symbol + 'm', newCandle);
            _activeSymbols[symbol] = currentCandleTime;
        }
        else
        {
            Console.WriteLine($"Last candle not in time series. Current time: {currentCandleTime} Previous: {lastCandleTime}");
            Console.WriteLine($"There are {(currentCandleTime - lastCandleTime) / 60_000} candles to be updated");
            Console.WriteLine($"Retrieving max limit of candles that are missing..");
            AddRequestToQue(Priority.High, symbol, lastCandleTime + MSECONDS_IN_MINUTE, 0, true);
        }
    }

    private async Task closeWebSocket()
    {
        Console.WriteLine("Closing Websocket connection");
        if(_ws != null)
            await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
        _resetWebsocket = false;
    }

    private void AddRequestToQue(Priority priority, string symbol, long startTime, long endTime, bool isMostRecentCandles)
    {
        ApiRequest request = new(symbol, startTime, endTime, isMostRecentCandles);
        Console.WriteLine($"Adding request to que {request}");
        _apiQue.Enqueue(request, priority);
        if(!_queIsProcessing)
            ProcessQue();
    }

    private async Task ProcessQue()
    {
        _queIsProcessing = true;
        Console.WriteLine("Starting que");
        while(_apiQue.Count > 0)
        {
            ApiRequest request = _apiQue.Dequeue();
            Console.WriteLine($"Process que request: {request}");
            var candles = await GetCandlesAsync(request.Symbol, request.StartTime, request.EndTime);
            
            if(candles == null) continue;

            // if last candle is not closed, remove it
            if(request.IsMostRecentCandles)
            {
                // remove unclosed candle
                candles.RemoveAt(candles.Count - 1);
                _activeSymbols[request.Symbol] = candles[candles.Count-1].Time;
            }
            
            int count = await _candleRepository.InsertCandles(request.Symbol + 'm', candles);
            
            Console.WriteLine($"Added {count} candles to db");

            // wait 2000ms between requests
            await Task.Delay(2000);
        }
        _queIsProcessing = false;
        Console.WriteLine("Ending que");
    }
}

// TODO
// 3. Start an API updater to load earlier candles
