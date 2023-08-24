namespace JoltXServer.Models;

public record SymbolType
{
    public int SymbolTypeId { get; set; }

    public required string Name { get; set; }

    public static bool Validate(SymbolType symbolType)
    {
        return symbolType.SymbolTypeId > 0
                && symbolType.Name != null
                && symbolType.Name.Length > 0
                && symbolType.Name.Length <= 14;
    }
}