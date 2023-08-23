namespace JoltXServer.Services;

using Newtonsoft.Json;
using JoltXServer.Models;

public static class BinanceService
{
    private static readonly string BinanceUrl = "https://api3.binance.com/api/v3/";

    public static async Task<List<Candle>?> GetCandlesAsync(string symbol, long startTime = 0, long endTime = 0)
    {
        string requestUrl = BinanceUrl + $"klines?symbol={symbol}&interval=1m&limit=1000";
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
    
}

