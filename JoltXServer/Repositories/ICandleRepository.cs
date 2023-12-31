using JoltXServer.Models;

namespace JoltXServer.Repositories;

public interface ICandleRepository
{
    public Task<List<Candle>?> GetCandlesAsync(string symbol, char interval, long startTime, long endTime);



}