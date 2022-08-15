using System;
using System.Collections.Generic;

namespace WarehouseApp.Models;

public class Stock : BaseModel
{
    public DateTime ExpirationDate { get; set; }

    public int ItemId { get; set; }

    public int Amount { get; set; }

    public Item? Item { get; set; }

    public List<StockInputLog>? StockInputLogs { get; set; }

    public List<StockOutputLog>? StockOutputLogs { get; set; }
}
