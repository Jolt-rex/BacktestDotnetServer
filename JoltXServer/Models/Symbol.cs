namespace JoltXServer.Models;

public record Symbol
{
    public int SymbolId { get; set; }
    public string? Name { get; set; }
    public bool IsActive { get; set; }
    public int StrategyCount { get; set; }
    public int Populartiy { get; set; }
    public int SymbolTypeId { get; set; }

    public static bool Validate(Symbol symbol)
    {
        return symbol != null 
            && symbol.Name != null
            && symbol.Name.Length > 0
            && symbol.Name.Length <= 10
            && symbol.SymbolTypeId > 0
            && symbol.SymbolTypeId <= 4;
    }
}