using System.Collections.Generic;

namespace WarehouseApp.Models;

public class Item : BaseModel
{
    public string Barcode { get; set; }

    public string Name { get; set; }

    public string Quantifier { get; set; }

    public int TotalAmount { get; set; } = 0;

    public List<Stock>? Stocks { get; set; }

    public Item(string barcode, string name, string quantifier, int totalAmount = 0)
    {
        Barcode = barcode;
        Name = name;
        Quantifier = quantifier;
        TotalAmount = totalAmount;
    }
}
