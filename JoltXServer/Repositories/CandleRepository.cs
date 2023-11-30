using JoltXServer.Models;
using JoltXServer.DataAccessLayer;
using JoltXServer.Repositories;

namespace JoltXServer.Repositories;

public class CandleRepository : ICandleRepository
{
    private IDatabaseSqlite _dbConnection;
    public CandleRepository(IDatabaseSqlite dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<List<Candle>?> GetCandlesAsync(string symbol, long startTime = 0, long endTime = 0)
    {
        _dbConnection.GetCandles


    }
}