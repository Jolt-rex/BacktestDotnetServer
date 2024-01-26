using JoltXServer.Models;

namespace JoltXServer.Repositories;

public interface ICandleRepository
{
    public Task<int> ValidateCandleTimeSeries(string symbol);
    public Task<List<Candle>?> GetCandlesAsync(string symbol, char interval, long startTime, long endTime);
    public Task<long> GetMostRecentCandleTime(string symbol);
    public Task<long> GetEarliestCandleTime(string symbol);

    public Task<int> InsertCandles(string symbolNameAndTime, List<Candle> candles);
    public Task<int> InsertOneCandle(string symbolNameAndTime, Candle candle);

}