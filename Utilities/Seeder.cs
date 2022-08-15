using System;
using System.Collections.Generic;
using WarehouseApp.Database;
using WarehouseApp.Models;

namespace WarehouseApp.Utilities;

public class Seeder
{
    public static void Seed()
    {
        using DatabaseContext dbContext = new();

        dbContext.AddRange(
            new User(username: "admin", name: "Admin", password: "password", isAdmin: true),
            new User(username: "user", name: "User", password: "password", isAdmin: false)
        );

        dbContext.AddRange(
            new Item(barcode: "8991038775416", name: "Lorem ipsum ", quantifier: "dus")
            {
                Stocks = new List<Stock>()
                {
                    new Stock()
                    {
                        ItemId = 1,
                        ExpirationDate = DateTime.Now.AddDays(10),
                        StockInputLogs = new List<StockInputLog>()
                        {
                            new StockInputLog()
                            {
                                Amount = 10,
                                UserId = 1,
                                StockId = 1
                            },
                            new StockInputLog()
                            {
                                Amount = 20,
                                UserId = 1,
                                StockId = 1
                            },
                            new StockInputLog()
                            {
                                Amount = 15,
                                UserId = 1,
                                StockId = 1
                            }
                        }
                    }
                }
            },
            new Item(
                barcode: "8991038775417",
                name: "Dolor sit amet",
                quantifier: "kg",
                totalAmount: 0
            ),
            new Item(
                barcode: "8991038775418",
                name: "Consectetur adipiscing",
                quantifier: "m",
                totalAmount: 0
            )
        );

        dbContext.SaveChanges();
    }
}
