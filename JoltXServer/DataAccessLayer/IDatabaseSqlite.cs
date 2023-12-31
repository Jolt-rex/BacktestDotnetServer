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
    public Task<int> ActivateSymbolById(int id);
    // END SYMBOLS

    // CANDLES
    public Task InsertCandles(string symbolNameAndTime, List<Candle> candles);
    public Task<long> GetEarliestCandleTime(string symbolName);
    public Task<List<Candle>?> GetCandles(string symbol, char interval, long startTime, long endTime);
    // END CANDLES
}