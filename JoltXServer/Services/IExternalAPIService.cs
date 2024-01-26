namespace JoltXServer.Services;

using JoltXServer.Models;

public interface IExternalAPIService
{
     public Task ActivateOneSymbol(string symbol, bool isStartup = false);
     public Task<List<Candle>?> GetCandlesAsync(string symbol, long startTime = 0);
}