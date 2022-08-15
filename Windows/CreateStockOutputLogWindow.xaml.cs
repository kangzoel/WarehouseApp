using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.EntityFrameworkCore;
using WarehouseApp.Database;
using WarehouseApp.Models;

namespace WarehouseApp.Windows;

/// <summary>
/// Interaction logic for CreateStockOutputLogWindow.xaml
/// </summary>
public partial class CreateStockOutputLogWindow : Window
{
    private List<Stock> _stockList = new();
    private List<StockOutputLog> _stockOutputLogList = new();

    public CreateStockOutputLogWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(
            DispatcherPriority.Background,
            () =>
            {
                using DatabaseContext dbContext = new();

                _stockList = dbContext.Stocks
                    .Where(x => x.Amount != 0)
                    .Include(x => x.Item)
                    .OrderBy(x => x.Item!.Name)
                    .ThenBy(x => x.ExpirationDate)
                    .ToList();

                StocksDataGrid.ItemsSource = _stockList;
            }
        );
    }

    private void StocksDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (StocksDataGrid.SelectedItem is Stock)
        {
            AddButton.IsEnabled = true;
            RemoveButton.IsEnabled = false;
        }
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedStock = StocksDataGrid.SelectedItem as Stock;

        OutputItemInputWindow window =
            new(
                StocksDataGrid,
                StockOutputLogsDataGrid,
                StocksDataGrid.SelectedIndex,
                selectedStock!,
                _stockOutputLogList
            )
            {
                Owner = this
            };
        window.Closed += (object? sender, EventArgs e) =>
        {
            SaveButton.IsEnabled = _stockList.Count > 0;
        };
        window.ShowDialog();
    }

    private void StockSearchTB_TextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = StockSearchTB.Text;

        StocksDataGrid.ItemsSource = _stockList
            .Where(
                x =>
                    x.Item!.Name.ToUpper().Contains(searchText.ToUpper())
                    || x.Item.Barcode.Contains(searchText)
            )
            .OrderBy(x => x.Item!.Name)
            .ThenBy(x => x.ExpirationDate)
            .ToList();
    }

    private void OutputSearchTB_TextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = OutputSearchTB.Text;

        StockOutputLogsDataGrid.ItemsSource = _stockOutputLogList
            .Where(
                x =>
                    x.Stock!.Item!.Name.ToUpper().Contains(searchText.ToUpper())
                    || x.Stock!.Item!.Barcode.Contains(searchText)
            )
            .OrderBy(x => x.Stock!.Item!.Name)
            .ThenBy(x => x.Stock!.ExpirationDate)
            .ToList();
    }

    private void StockOutputLogsDataGrid_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e
    )
    {
        RemoveButton.IsEnabled = StockOutputLogsDataGrid.SelectedItem is StockOutputLog;
    }

    private void RemoveButton_Click(object sender, RoutedEventArgs e)
    {
        if (StockOutputLogsDataGrid.SelectedItem is StockOutputLog selectedItem)
        {
            var stock = _stockList.Single(x => x.Id == selectedItem.Stock!.Id);

            stock.Amount += selectedItem.Amount;
            _stockOutputLogList.Remove(selectedItem);

            StocksDataGrid.Items.Refresh();
            StockOutputLogsDataGrid.Items.Refresh();
        }

        if (_stockOutputLogList.Count == 0)
            SaveButton.IsEnabled = false;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        SaveButton.IsEnabled = false;

        Dispatcher.BeginInvoke(
            DispatcherPriority.Background,
            new Action(() =>
            {
                using DatabaseContext dbContext = new();

                try
                {
                    using var transaction = dbContext.Database.BeginTransaction();

                    foreach (var _stockOutputLog in _stockOutputLogList)
                    {
                        var stock = dbContext.Stocks
                            .Include(x => x.Item)
                            .First(x => x.Id == _stockOutputLog.Stock!.Id);

                        stock.Amount -= _stockOutputLog.Amount;
                        stock.Item!.TotalAmount -= _stockOutputLog.Amount;

                        _stockOutputLog.Stock = null;
                    }

                    dbContext.AddRange(_stockOutputLogList);

                    transaction.Commit();

                    MessageBox.Show(
                        caption: nameof(MessageBoxImage.Information),
                        button: MessageBoxButton.OK,
                        messageBoxText: "Barang keluar berhasil dicatat",
                        icon: MessageBoxImage.Information
                    );

                    dbContext.SaveChanges();

                    Close();
                }
                catch (Exception error)
                {
                    SaveButton.IsEnabled = true;

                    MessageBox.Show(
                        caption: nameof(MessageBoxImage.Error),
                        button: MessageBoxButton.OK,
                        messageBoxText: $"Barang keluar gagal dicatat. {error.Message}",
                        icon: MessageBoxImage.Error
                    );
                }
            })
        );
    }
}
