using JoltXServer.Models;

namespace JoltXServer.Repositories;

public interface ICandleRepository
{
    public Task<List<Candle>?> GetCandlesAsync(string symbol, long startTime, long endTime);

    

}