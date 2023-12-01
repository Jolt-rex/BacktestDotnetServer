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
            && ValidateName(symbol.Name)            
            && symbol.SymbolTypeId > 0
            && symbol.SymbolTypeId <= 4;
    }

    public static bool ValidateName(string? name)
    {
        return name != null
            && name.Length > 0
            && name.Length <= 10;
    }
}