namespace WarehouseApp.Models;

public class StockInputLog : BaseModel
{
    public int StockId { get; set; }

    public int UserId { get; set; }

    public int Amount { get; set; }

    public Stock? Stock { get; set; }

    public User? User { get; set; }
}
