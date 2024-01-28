using JoltXServer.Models;

namespace JoltXServer.Repositories;

public interface ICandleRepository
{
    public Task<int> ValidateCandleTimeSeries(string symbol);
    public Task<List<Candle>?> GetCandlesAsync(string symbol, char interval, long startTime, long endTime);
    public Task<long> GetMostRecentCandleTime(string symbol);
    public Task<long> GetEarliestCandleTime(string symbol);

    public Task<int> InsertCandles(string symbol, List<Candle> candles);
    public Task<int> InsertOneCandle(string symbol, Candle candle);

}