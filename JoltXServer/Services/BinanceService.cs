
using Newtonsoft.Json;
using JoltXServer.Models;
using JoltXServer.DataAccessLayer;

namespace JoltXServer.Services;

public class BinanceService : IExternalAPIService
{
    private static readonly int SECONDS_IN_HOUR = 3600;
    private static readonly int SECONDS_IN_MINUTE = 60;
    // TODO update this limit rate from API regularly
    private static int _binanceCandleLimitPerRequest = 1500;

    private static readonly IDictionary<string, int> _lastCandleTime = new Dictionary<string, int>();

    private static readonly string _binanceUrl = "https://api3.binance.com/api/v3";
    // `${klineEndpoint}?symbol=${s}&interval=${timeFrame}&limit=${API_KLINE_LIMIT}`
    // `&startTime=${startTime.toString()}&endTime=${endTime}
    private static string _binanceWebSocketUrl = "wss://stream.binance.com:9443/stream?streams=";
    // wss://stream.binance.com:9443/stream?streams=ethbtc@kline1m/linkusdt@kline1m
    private readonly IDatabaseSqlite _dbConnection;


    public BinanceService(IDatabaseSqlite dbSqlite)
    {
        _dbConnection = dbSqlite;
    }

    // add candles to websocket, and retrieve past 1 year of historical candles
    public async Task<int> ActivateSymbols(string[] symbols)
    {
        int i = 0;
        for(; i < symbols.Length; i++)
        {

            _lastCandleTime.Add(symbols[i], 0);
            if(_binanceWebSocketUrl[^1] != '=') _binanceWebSocketUrl += '/';
            _binanceWebSocketUrl += $"{symbols[i]}@kline1m";
        }
        await RestartWebSocket();

        return i;
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
        int interval = symbol[^1] == 'h' ? SECONDS_IN_HOUR : SECONDS_IN_MINUTE;
        long endTime = await _dbConnection.GetEarliestCandleTime(symbol);

        // if there are no current candles in database, set endTime to now
        // otherwise, subtract interval time to get previous candle endTime
        if(endTime == 0) endTime = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        else endTime -= interval;

        if((endTime - startTime) / interval > 1000)
        {
            // use generator as request limit is 1500 TODO
            return -1;
        }
        
        // else make request directly
        var candles = await GetCandlesAsync(symbol, startTime, endTime);

        if(candles == null) return 0;
        
        await _dbConnection.InsertCandles(symbol, candles);

        return candles.Count;
    }

    // websocket
    // SELECT id FROM table ORDER BY id DESC LIMIT 0,1 - to get row with highest id

    private void RestartWebSocket()
    {

    }
    
}

