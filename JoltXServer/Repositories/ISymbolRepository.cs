using JoltXServer.Models;
using JoltXServer.DataAccessLayer;

namespace JoltXServer.Repository;


public interface ISymbolRepository
{
    // GetAll
    public Task<List<Symbol>?> GetAll();

    public Task<List<SymbolType>?> GetAllTypes();

    // GetById
    public Task<Symbol> GetById(int id);
    public Task<Symbol> GetByName(string name);

    // CreateNew
    public Task<int> CreateNew(Symbol symbol);

    // // UpdateById
    public Task<int> UpdateById(Symbol symbol);

    // // DeleteById
    // public Task<int> DeleteById(int id);

    // // IncrementStrategyCount
    // public Task<int> IncrementStrategyCount(int id);

}