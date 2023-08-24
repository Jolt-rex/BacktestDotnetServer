using JoltXServer.DataAccessLayer;
using JoltXServer.Models;
using JoltXServer.Repository;

namespace JoltXServer.Repository;

public class SymbolRepository : ISymbolRepository
{
    private readonly IDatabaseSqlite DbConnection;

    public SymbolRepository(IDatabaseSqlite dbConnection)
    {
        DbConnection = dbConnection;
    }
    public async Task<List<Symbol>?> GetAll()
    {
        return await DbConnection.GetAllSymbols();
    }

    // // GetById
    public async Task<Symbol> GetById(int id)
    {
        return await DbConnection.GetSymbolById(id);
    }

    // CreateNew
    public async Task<int> CreateNew(Symbol symbol)
    {
        return await DbConnection.CreateSymbol(symbol);
    }

    // // UpdateById
    // public Task<int> UpdateById(int id, Symbol symbol);

    // // DeleteById
    // public Task<int> DeleteById(int id);

    // // IncrementStrategyCount
    // public Task<int> IncrementStrategyCount(int id);
}