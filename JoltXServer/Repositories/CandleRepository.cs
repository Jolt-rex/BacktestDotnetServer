using JoltXServer.Models;
using JoltXServer.DataAccessLayer;

namespace JoltXServer.Repositories;

public class CandleRepository : ICandleRepository
{
    private IDatabaseSqlite _dbConnection;
    public CandleRepository(IDatabaseSqlite dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<int> ValidateCandleTimeSeries(string symbol)
    {
        return await _dbConnection.ValidateCandleTimeSeries(symbol);
    }

    public async Task<List<Candle>?> GetCandlesAsync(string symbol, char interval, long startTime = 0, long endTime = 0)
    {
        return await _dbConnection.GetCandles(symbol, interval, startTime, endTime);
    }

    public async Task<long> GetMostRecentCandleTime(string symbol)
    {
        return await _dbConnection.GetMostRecentCandleTime(symbol);
    }

    public async Task<long> GetEarliestCandleTime(string symbol)
    {
        return await _dbConnection.GetEarliestCandleTime(symbol);
    }

    public async Task<int> InsertCandles(string symbolNameAndTime, List<Candle> candles)
    {
        return await _dbConnection.InsertCandles(symbolNameAndTime, candles);
    }

    public async Task<int> InsertOneCandle(string symbolNameAndTime, Candle candle)
    {
        return await _dbConnection.InsertOneCandle(symbolNameAndTime, candle);
    }
}