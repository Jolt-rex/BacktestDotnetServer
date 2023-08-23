
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace JoltXServer.Models;

public record Symbol
{
    public int SymbolId { get; set; }
    public string? Name { get; set; }
    public bool IsActive { get; set; }
    public int StrategyCount { get; set; }

    public static bool Validate(Symbol symbol)
    {
        if(symbol != null 
            && symbol.Name != null
            && symbol.Name.Length > 0
            && symbol.Name.Length <= 10)
            return true;

        return false;
    }
}