namespace JoltXServer.Services;

using JoltXServer.Models;

public interface IExternalAPIService
{
     public Task<List<Candle>?> GetCandlesAsync(string symbol, long startTime = 0, long endTime = 0);

     public Task<int> FetchAndSaveEarlierCandles(string symbol, long startTime);
}