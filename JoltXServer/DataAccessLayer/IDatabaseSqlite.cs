using JoltXServer.Models;

namespace JoltXServer.DataAccessLayer;

public interface IDatabaseSqlite
{
    public Task Connect();

    public Task<List<Symbol>> GetAllSymbols();

    public Task<int> CreateSymbol(Symbol symbol);
}