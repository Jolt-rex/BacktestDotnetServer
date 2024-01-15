
using Newtonsoft.Json;
using JoltXServer.Models;
using JoltXServer.DataAccessLayer;
using JoltXServer.Repositories;
using System.Net.WebSockets;
using Microsoft.VisualBasic;
using System.Text;

namespace JoltXServer.Services;

public class BinanceService : IExternalAPIService
{
    private static readonly int SECONDS_IN_HOUR = 3600;
    private static readonly int SECONDS_IN_MINUTE = 60;
    // TODO update this limit rate from API regularly
    private static int _binanceCandleLimitPerRequest = 1500;

    private static readonly IDictionary<string, long> _activeSymbols = new Dictionary<string, long>();

    private static readonly string _binanceUrl = "https://api3.binance.com/api/v3";
    // `${klineEndpoint}?symbol=${s}&interval=${timeFrame}&limit=${API_KLINE_LIMIT}`
    // `&startTime=${startTime.toString()}&endTime=${endTime}
    private static string _binanceWebSocketUrl = "wss://stream.binance.com:9443/stream?streams=btcusdt@kline1m";
    // wss://stream.binance.com:9443/stream?streams=ethbtc@kline1m/linkusdt@kline1m
    private static ClientWebSocket _ws;
    private readonly ISymbolRepository _symbolRepository;
    private readonly ICandleRepository _candleRepository;


    public BinanceService(ISymbolRepository symbolRepository, ICandleRepository candleRepository)
    {
        Console.WriteLine("Creating BinanceService object");
        _symbolRepository = symbolRepository;
        _candleRepository = candleRepository;
        RestartWebSocket();        
    }

    // add candles to websocket
    // retrieve most recent candles and add to repo
    // restart websocket
    public async Task<int> PreloadSymbol(string symbol)
    {

        _activeSymbols.Add(symbol, 0);

        if(_binanceWebSocketUrl[^1] != '=') _binanceWebSocketUrl += '/';
        _binanceWebSocketUrl += $"{symbol}@kline1m";

        List<Candle>? previousCandles = await GetCandlesAsync(symbol);

        if(previousCandles == null || previousCandles.Count == 0)
            return -1;

        int count = await _candleRepository.InsertCandles(symbol + 'c', previousCandles);

        _activeSymbols[symbol] = previousCandles[^1].Time;
        
        RestartWebSocket();

        return count;
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


    private async void RestartWebSocket()
    {
        _ws?.Dispose();

        _ws = new ClientWebSocket();
        Uri serviceUri = new Uri(_binanceWebSocketUrl);
        
        try
        {
            await _ws.ConnectAsync(serviceUri, CancellationToken.None);
        
            var receiveTask = Task.Run(async () =>
            {
                var buffer = new byte[1024 * 4];
                while(true)
                {
                    var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if(result.MessageType == WebSocketMessageType.Close) break;

                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine("Received: " + message);
                }
            });

            await receiveTask;
        }
        catch (WebSocketException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    
}

