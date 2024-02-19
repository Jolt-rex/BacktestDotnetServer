
namespace JoltXServer.Models;

public class Strategy
{
    public int Id { get; set; }

    public ICollection<Trade> Trades { get; set; } = new List<Trade>();
    public required string Name { get; set; }
    public string? BuyCondition { get; set; }
    public string? SellCondition { get; set; }
    public decimal PercentProfit { get; set; }
    public decimal NetProfit { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal GrossLoss { get; set; }
    public decimal MaxRunUp { get; set; }
    public decimal MaxDrawDown { get; set; }
    public decimal SharpeRatio { get; set; }
    public int ClosedTrades { get; set; }
    public int OpenTrades { get; set; }

} 
