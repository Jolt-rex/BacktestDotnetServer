namespace JoltXServer.Services;

using JoltXServer.Models;

public interface IExternalAPIService
{
     public Task ActivateOneSymbol(string symbol, bool isStartup = false);
     public Task<List<Candle>?> GetCandlesAsync(string symbol, long startTime = 0, long endTime = 0);

     public Task<List<Candle>> GetCandlesGeneratorAsync(string symbol, long startTime, long endTime = 0);

     public Task<int> FetchHistoricalCandles(string symbol, long startTime);
}