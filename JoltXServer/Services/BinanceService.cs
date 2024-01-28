
using Newtonsoft.Json;
using JoltXServer.Models;
using JoltXServer.Repositories;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json.Linq;

namespace JoltXServer.Services;

public class BinanceService : IExternalAPIService
{
    private enum Priority : ushort
    {
        High = 1,
        Medium = 2,
        Low = 3
    };

    private readonly struct ApiRequest
    {
        public ApiRequest(string symbol, long startTime, bool isMostRecent)
        {
            Symbol = symbol;
            StartTime = startTime;
            IsMostRecentCandles = isMostRecent;
        }

        public string Symbol { get; }
        public long StartTime { get; }
        public bool IsMostRecentCandles { get; }

        public override string ToString() => $"ApiRequest for symbol: {Symbol} start time: {StartTime} is most recent {IsMostRecentCandles}";
    }

    private static readonly int MSECONDS_IN_HOUR = 3_600_000;
    private static readonly int MSECONDS_IN_MINUTE = 60_000;
    // TODO update this limit rate from API regularly
    private static int _binanceCandleLimitPerRequest = 1000;

    // UNIX timestamp to target towards for earliest candle
    // initially set to 01/01/2023 00:00hrs
    private static long _firstCandleTimeTarget = 1_672_495_200_000;

    // _activeSymbols provides in memory data of the most recent and oldest candles
    // long[0] == earliest candle time, long[1] == most recent candle time
    private readonly IDictionary<string, long[]> _activeSymbols;
    private readonly HashSet<string> _queuedSymbols;

    private static readonly string _binanceUrl = "https://api3.binance.com/api/v3";
    // `${klineEndpoint}?symbol=${s}&interval=${timeFrame}&limit=${API_KLINE_LIMIT}`
    // `&startTime=${startTime.toString()}&endTime=${endTime}
    private static string _binanceWebSocketUrl = "wss://stream.binance.com/stream?streams=";
    // wss://stream.binance.com:9443/stream?streams=ethbtc@kline_1m/linkusdt@kline_1m
    private static ClientWebSocket? _ws;
    private static bool _resetWebsocket = false;

    private static PriorityQueue<ApiRequest, Priority> _apiQue;
    private static bool _queIsProcessing = false;

    private readonly ISymbolRepository _symbolRepository;
    private readonly ICandleRepository _candleRepository;


    public BinanceService(ISymbolRepository symbolRepository, ICandleRepository candleRepository)
    {
        Console.WriteLine("Creating BinanceService object");
        _symbolRepository = symbolRepository;
        _candleRepository = candleRepository;

        _apiQue = new PriorityQueue<ApiRequest, Priority>();
        _activeSymbols = new Dictionary<string, long[]>();
        _queuedSymbols = new();

        Startup();
    }

    private async void Startup()
    {
        await StartupActivateSymbols();
        WebSocketLoop();
        UpdateHistoricalCandles();
    }

    private async Task StartupActivateSymbols()
    {

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
        long mostRecentCandleTime = await _candleRepository.GetMostRecentCandleTime(symbol);
        long earliestCandleTime = await _candleRepository.GetEarliestCandleTime(symbol);
        _activeSymbols.Add(symbol, new long[2] {earliestCandleTime, mostRecentCandleTime});

        _resetWebsocket = !isStartup;
    }

    public async Task<List<Candle>?> GetCandlesAsync(string symbol, long startTime = 0)
    {
        string requestUrl = _binanceUrl + $"/klines?symbol={symbol}&interval=1m&limit={_binanceCandleLimitPerRequest}";
        if(startTime > 0) requestUrl += $"&startTime={startTime}";

        HttpClient client = new();        
        HttpResponseMessage response = await client.GetAsync(requestUrl);

        if(!response.IsSuccessStatusCode)
            return null;

        var jsonResponse = await response.Content.ReadAsStringAsync();        

        List<Candle>? candleSticks = JsonConvert.DeserializeObject<List<Candle>>(jsonResponse);

        return candleSticks;
    }

    private JToken? ParseBuffer(byte[] buffer, int size)
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

    private async void WebSocketLoop()
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

                    var candleData = ParseBuffer(buffer, result.Count);

                    // if the websocket candle is closed, add the candle
                    if(candleData != null && (bool)candleData["x"])
                        AddCandle(candleData);          

                    if(_resetWebsocket == true) break;
                }
                await CloseWebSocket();
            }
        }
    }

    private async void AddCandle(JToken candleData)
    {
        string symbol = (string)candleData["s"];
        long lastCandleTime = _activeSymbols[(string)candleData["s"]][1];
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
            await _candleRepository.InsertOneCandle(symbol, newCandle);
            _activeSymbols[symbol][1] = currentCandleTime;
            if(_activeSymbols[symbol][0] == 0)
                _activeSymbols[symbol][0] = currentCandleTime;
        }
        else
        {
            Console.WriteLine($"There are {(currentCandleTime - lastCandleTime) / 60_000} candles to be updated");
            await AddRequestToQue(Priority.High, symbol, lastCandleTime + MSECONDS_IN_MINUTE, true);
        }
    }

    private static async Task CloseWebSocket()
    {
        Console.WriteLine("Closing Websocket connection");
        if(_ws != null)
            await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
        _resetWebsocket = false;
    }

    private async Task AddRequestToQue(Priority priority, string symbol, long startTime, bool isMostRecentCandles)
    {
        ApiRequest request = new(symbol, startTime, isMostRecentCandles);
        Console.WriteLine($"Adding request to que {request}");
        _apiQue.Enqueue(request, priority);
        if(!_queIsProcessing)
            _ = ProcessQue();
    }

    private async Task ProcessQue()
    {
        _queIsProcessing = true;
        // Console.WriteLine($"Starting que");
        while(_apiQue.Count > 0)
        {
            Console.WriteLine($"Processing que item with {_apiQue.Count} items");
            Console.WriteLine("Current que items");
            foreach(var apiRequest in _apiQue.UnorderedItems)
            {
                Console.WriteLine(apiRequest);
            }
            ApiRequest request = _apiQue.Dequeue();
            Console.WriteLine($"Process que request: {request}");
            var candles = await GetCandlesAsync(request.Symbol, request.StartTime);
            
            if(candles == null) continue;

            // if last candle is not closed, remove it
            if(request.IsMostRecentCandles)
                // remove unclosed candle
                candles.RemoveAt(candles.Count - 1);
            
            int count = await _candleRepository.InsertCandles(request.Symbol, candles);
            
            if(request.IsMostRecentCandles)
                _activeSymbols[request.Symbol][1] = candles[^1].Time;
            else
                _queuedSymbols.Remove(request.Symbol);
            
            if(_activeSymbols[request.Symbol][0] == 0 || !request.IsMostRecentCandles)
                _activeSymbols[request.Symbol][0] = candles[0].Time;


            Console.WriteLine($"Added {count} candles to db");

            // wait 2000ms between requests
            await Task.Delay(2000);
        }
        _queIsProcessing = false;
        Console.WriteLine("Ending que");
    }

    // Updater to request historical candle data
    private async void UpdateHistoricalCandles()
    {
        while(true)
        {
            foreach(KeyValuePair<string, long[]> symbol in _activeSymbols)
            {
                if(symbol.Value[0] > _firstCandleTimeTarget && !_queuedSymbols.Contains(symbol.Key) && _apiQue.Count < 1024)
                {
                    long firstCandleTime = symbol.Value[0] - (_binanceCandleLimitPerRequest * MSECONDS_IN_MINUTE);
                    _queuedSymbols.Add(symbol.Key);
                    _ = AddRequestToQue(Priority.Low, symbol.Key, firstCandleTime, false);
                }

                
                // int validatedCandleCount = await _candleRepository.ValidateCandleTimeSeries(symbol.Key);
                // Console.WriteLine($"Checking timeseries data for {symbol.Key} : {validatedCandleCount}");
            }
            await Task.Delay(5000);
        }
    }
}

// TODOs
