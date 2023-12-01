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

    public async Task<List<Candle>?> GetCandlesAsync(string symbol, char interval, long startTime = 0, long endTime = 0)
    {
        return await _dbConnection.GetCandles(symbol, interval, startTime, endTime);

    }
}