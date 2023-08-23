using JoltXServer.Models;

namespace JoltXServer.Repository;


public interface ISymbolRepository
{
    // GetAll
    public Task<List<Symbol>> GetAll();

    // GetById
    // public Task<Symbol> GetById(int id);

    // CreateNew
    public Task<int> CreateNew(Symbol symbol);

    // // UpdateById
    // public Task<int> UpdateById(int id, Symbol symbol);

    // // DeleteById
    // public Task<int> DeleteById(int id);

    // // IncrementStrategyCount
    // public Task<int> IncrementStrategyCount(int id);

}