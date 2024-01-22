
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

namespace JoltXServer.Services;

public class BinanceService : IExternalAPIService
{
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

    private static bool _resetWebsocket = false;
    private readonly ISymbolRepository _symbolRepository;
    private readonly ICandleRepository _candleRepository;


    public BinanceService(ISymbolRepository symbolRepository, ICandleRepository candleRepository)
    {
        Console.WriteLine("Creating BinanceService object");
        _symbolRepository = symbolRepository;
        _candleRepository = candleRepository;

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

    public async Task<List<Candle>> GetCandlesGeneratorAsync(string symbol, long startTime, long endTime = 0)
    {
        string requestUrl = _binanceUrl + $"klines?symbol={symbol}&interval=1m";

        HttpClient client = new();
        HttpResponseMessage response = await client.GetAsync(requestUrl);

        // TODO
        return new List<Candle>();
    }

    // gets earlier candles from startTime and ends at either current time, or the 
    // first time already in the database
    public async Task<int> FetchHistoricalCandles(string symbol, long startTime)
    {
        return 1;
        // int interval = symbol[^1] == 'h' ? SECONDS_IN_HOUR : SECONDS_IN_MINUTE;
        // long endTime = await _dbConnection.GetEarliestCandleTime(symbol);

        // // if there are no current candles in database, set endTime to now
        // // otherwise, subtract interval time to get previous candle endTime
        // if(endTime == 0) endTime = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        // else endTime -= interval;

        // if((endTime - startTime) / interval > _binanceCandleLimitPerRequest)
        // {
        //     // use generator as request limit is 1500 TODO
        //     return -1;
        // }
        
        // // else make request directly
        // var candles = await GetCandlesAsync(symbol, startTime, endTime);

        // if(candles == null) return 0;
        
        // await _dbConnection.InsertCandles(symbol, candles);

        // return candles.Count;
    }

    // websocket
    // SELECT id FROM table ORDER BY id DESC LIMIT 0,1 - to get row with highest id

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
                    if(candleData != null && (bool)candleData["x"])
                    {
                        // Console.WriteLine($"1m candle for {candleData["s"]}");
                        // Console.WriteLine(candleData["t"]);
                        // Console.WriteLine(candleData["o"]);
                        // Console.WriteLine(candleData["c"]);
                        // Console.WriteLine($"Buffer size: {result.Count}");
                        await addCandle(candleData);       
                    }                        
                    

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
        if(lastCandleTime == currentCandleTime - MSECONDS_IN_MINUTE || lastCandleTime == 0)
        {
            Candle newCandle = new()
            {
                Time = (long)candleData["t"],
                Open = (decimal)candleData["o"],
                High = (decimal)candleData["h"],
                Low = (decimal)candleData["l"],
                Close = (decimal)candleData["c"],
                Volume = (decimal)candleData["q"]
            };
            await _candleRepository.InsertOneCandle(symbol + 'm', newCandle);
            _activeSymbols[symbol] = currentCandleTime;
        }
        else
            Console.WriteLine($"Last candle not in time series. Current time: {currentCandleTime} Previous: {lastCandleTime}");
            Console.WriteLine($"There are {(currentCandleTime - lastCandleTime) / 60_000} candles to be updated");
            Console.WriteLine($"Retrieving max limit of candles that are missing..");
            var candles = await GetCandlesAsync(symbol, lastCandleTime + MSECONDS_IN_MINUTE, 0);
            Console.WriteLine($"First obtained candle time: {candles[0].Time}");
            int count = await _candleRepository.InsertCandles(symbol + 'm', candles);
            _activeSymbols[symbol] = candles[candles.Count-1].Time;
            Console.WriteLine($"Added {count} candles to db");
    }

    private async Task closeWebSocket()
    {
        Console.WriteLine("Closing Websocket connection");
        if(_ws != null)
            await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
        _resetWebsocket = false;
    }
}

// TODO
// 2. if most recent candle is lagging current candle time - API request in loop to update
// 3. Start an API updater to load earlier candles
