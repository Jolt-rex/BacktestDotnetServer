namespace JoltXServer.Services;

using Newtonsoft.Json;
using JoltXServer.Models;
using JoltXServer.DataAccessLayer;

public class BinanceService : IBinanceService
{
    private static readonly string BinanceUrl = "https://api3.binance.com/api/v3/";
    private readonly IDatabaseSqlite DbConnection;

    public BinanceService(IDatabaseSqlite dbSqlite)
    {
        DbConnection = dbSqlite;
    }

    public async Task<List<Candle>?> GetCandlesAsync(string symbol, long startTime = 0, long endTime = 0)
    {
        char interval = symbol[^1];
        symbol = symbol.Remove(symbol.Length - 1, 1);
        string requestUrl = BinanceUrl + $"klines?symbol={symbol}&interval=1{interval}&limit=1000";
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

    // gets earlier candles from startTime and ends at either current time, or the 
    // first time already in the database
    public async Task<int> FetchAndSaveEarlierCandles(string symbol, long startTime)
    {
        int interval = symbol[^1] == 'h' ? 3600 : 60;
        long endTime = await DbConnection.GetEarliestCandleTime(symbol);

        // if there are no current candles in database, set endTime to now
        // otherwise, subtract interval time to get previous candle endTime
        if(endTime == 0) endTime = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        else endTime -= interval;

        if((endTime - startTime) / interval > 1000)
        {
            // use generator as request limit is 1000 TODO
            return -1;
        }
        
        // else make request directly
        var candles = await GetCandlesAsync(symbol, startTime, endTime);
        await DbConnection.InsertCandles(symbol, candles);

        return candles.Count;
    }
    
}

