
namespace JoltXServer.Models;

public class Trade
{
    public int Id { get; set; }
    public required string Type {get; set; }
    public required string Signal { get; set; }
    public long EntryTime { get; set; }
    public long ExitTime { get; set; }
    public decimal EntryPrice { get; set; }
    public decimal ExitPrice { get; set; }
    public decimal Profit { get; set; }
    public decimal PercentProfit { get; set; }
    public decimal CumProfit { get; set; }
}