using JoltXServer.Models;

namespace JoltXServer.DataAccessLayer;

public interface IDatabaseSqlite
{
    public Task Connect();

    // SYMBOLS 
    public Task<List<Symbol>?> GetAllSymbols();
    public Task<List<SymbolType>?> GetAllSymbolTypes();

    public Task<Symbol> GetSymbolById(int id);
    public Task<Symbol> GetSymbolByName(string name);
    public Task<int> CreateSymbol(Symbol symbol);
    public Task<int> UpdateSymbolById(Symbol symbol);
    public Task<int> ActivateSymbolByName(string name);
    public Task<string[]> GetAllActiveSymbolNames();
    // END SYMBOLS

    // CANDLES
    public Task<int> ValidateCandleTimeSeries(string symbol);
    public Task<int> InsertOneCandle(string symbol, Candle candle);
    public Task<int> InsertCandles(string symbol, List<Candle> candles);
    public Task<long> GetMostRecentCandleTime(string symbolName);
    public Task<long> GetEarliestCandleTime(string symbolName);
    public Task<List<Candle>?> GetCandles(string symbol, long startTime, long endTime);
    // END CANDLES
}