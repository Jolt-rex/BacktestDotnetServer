using JoltXServer.DataAccessLayer;
using JoltXServer.Models;

namespace JoltXServer.Repositories;

public class SymbolRepository : ISymbolRepository
{
    private readonly IDatabaseSqlite _dbConnection;

    public SymbolRepository(IDatabaseSqlite dbConnection)
    {
        _dbConnection = dbConnection;
    }
    public async Task<List<Symbol>?> GetAll()
    {
        return await _dbConnection.GetAllSymbols();
    }

    public async Task<List<SymbolType>?> GetAllTypes()
    {
        return await _dbConnection.GetAllSymbolTypes();
    }

    // // GetById
    public async Task<Symbol> GetById(int id)
    {
        return await _dbConnection.GetSymbolById(id);
    }

    public async Task<Symbol> GetByName(string name)
    {
        return await _dbConnection.GetSymbolByName(name);
    }

    // CreateNew
    public async Task<int> CreateNew(Symbol symbol)
    {
        return await _dbConnection.CreateSymbol(symbol);
    }

    // UpdateById
    public async Task<int> UpdateById(Symbol symbol)
    {
        return await _dbConnection.UpdateSymbolById(symbol);
    }

    public async Task<int> ActivateByName(string name)
    {
        return await _dbConnection.ActivateSymbolByName(name);
    }

    // // DeleteById
    // public Task<int> DeleteById(int id);

    // // IncrementStrategyCount
    // public Task<int> IncrementStrategyCount(int id);
}