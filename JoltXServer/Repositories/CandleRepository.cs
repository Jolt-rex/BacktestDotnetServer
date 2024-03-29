using JoltXServer.Models;
using JoltXServer.DataAccessLayer;

namespace JoltXServer.Repositories;

public class CandleRepository : ICandleRepository
{
    private IDatabaseSqlite _dbConnection;

    private readonly Dictionary<string, List<Candle>?> _cachedCandles;
    public CandleRepository(IDatabaseSqlite dbConnection)
    {
        _dbConnection = dbConnection;
        _cachedCandles = new();
    }

    public async Task<int> ValidateCandleTimeSeries(string symbol)
    {
        return await _dbConnection.ValidateCandleTimeSeries(symbol);
    }

    // TODO use in memory Dictionary <symbolInterval, List<Candle> to cache candles from db for faster response
    // eg. <"BTCUSDT1M" : [Candle1, Candle2, Candle3]>
    public async Task<List<Candle>?> GetCandlesAsync(string symbol, string interval = "", long startTime = 0, long endTime = 0, int limit = 1000)
    {
        
        // 1M and 1H candles in memory and Get ability to retrieve 1M 5M 15M 30M 1H 2H 4H 6H 8H 12H 1D 1W
        return await _dbConnection.GetCandles(symbol, startTime, endTime, limit);

        // string symbolInterval = symbol + interval;

        // if(!await AddCandlesToCache(symbol, interval)) return null;

        // if(startTime == 0 && endTime == 0 && limit == 0)
        //     return _cachedCandles[symbolInterval];

        // int count = _cachedCandles[symbolInterval].Count;
        // if(startTime == 0 && endTime == 0)
        // {
        //     int startIndex = Math.Max(0, count - limit - 1);
        //     int candleCountToReturn = limit;
        //     if(candleCountToReturn > count - startIndex - 1)
        //         candleCountToReturn = count - startIndex;
        //     return _cachedCandles[symbolInterval]?.GetRange(startIndex, candleCountToReturn);
        // }
        

        // long lastTimeCandles = _cachedCandles[symbolInterval][^1].Time;
        // long startTimeCandles = _cachedCandles[symbolInterval][0].Time;
        
        
        // return _cachedCandles[symbolInterval];
    }

    private async Task<bool> AddCandlesToCache(string symbol, string interval)
    {
        if(!_cachedCandles.ContainsKey(symbol + interval))
        {
            List<Candle>? candles = await _dbConnection.GetCandles(symbol,0,0,0);
            if(candles == null || candles.Count == 0) return false;
            _cachedCandles.Add(symbol+interval, candles);
        }
        return true;
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