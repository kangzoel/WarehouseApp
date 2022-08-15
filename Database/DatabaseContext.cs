using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using WarehouseApp.Models;

namespace WarehouseApp.Database;

public class DatabaseContext : DbContext
{
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<StockInputLog> StockInputLogs => Set<StockInputLog>();
    public DbSet<StockOutputLog> StockOutputLogs => Set<StockOutputLog>();
    public DbSet<User> Users => Set<User>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSqlite("Data Source=database.sqlite")
            .EnableSensitiveDataLogging()
            .LogTo(e => Trace.WriteLine(e))
            ;
    }

    public override int SaveChanges()
    {
        var entries = ChangeTracker.Entries();

        foreach (var entry in entries)
        {
            if (entry.Entity is BaseModel model)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        model.CreatedAt = DateTime.Now;
                        model.UpdatedAt = DateTime.Now;
                        break;
                    case EntityState.Modified:
                        model.UpdatedAt = DateTime.Now;
                        break;
                }
            }

            if (entry.Entity is User user)
            {
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                }
            }

            if (entry.Entity is StockInputLog stockInputLog)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        stockInputLog.Stock!.Amount += stockInputLog.Amount;
                        stockInputLog.Stock!.Item!.TotalAmount += stockInputLog.Amount;
                        break;
                    case EntityState.Deleted:
                        stockInputLog.Stock!.Amount -= stockInputLog.Amount;
                        stockInputLog.Stock!.Item!.TotalAmount -= stockInputLog.Amount;
                        break;
                }
            }

            if (entry.Entity is StockOutputLog stockOutputLog)
            {
                switch (entry.State)
                {
                    case EntityState.Deleted:
                        stockOutputLog.Stock!.Amount += stockOutputLog.Amount;
                        stockOutputLog.Stock!.Item!.TotalAmount += stockOutputLog.Amount;
                        break;
                }
            }
        }

        return base.SaveChanges();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Stock>().HasAlternateKey(s => new { s.ItemId, s.ExpirationDate });
    }
}
