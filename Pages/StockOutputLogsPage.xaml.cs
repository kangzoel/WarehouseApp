using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.EntityFrameworkCore;
using WarehouseApp.Database;
using WarehouseApp.Models;
using WarehouseApp.Types;
using WarehouseApp.Utilities;
using WarehouseApp.Utilities.Pdf;
using WarehouseApp.Windows;

namespace WarehouseApp.Pages;

/// <summary>
/// Interaction logic for StockOutputLogsPage.xaml
/// </summary>
public partial class StockOutputLogsPage : Page
{
    public StockOutputLogsPage()
    {
        InitializeComponent();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        if (Session.Instance.User!.IsAdmin == false)
            AddButton.Visibility = Visibility.Hidden;

        Dispatcher.BeginInvoke(
            DispatcherPriority.Background,
            new Action(() =>
            {
                using (var dbContext = new DatabaseContext())
                {
                    var minMax = dbContext.StockOutputLogs
                        .OrderBy(x => x.CreatedAt)
                        .GroupBy(x => x.CreatedAt.Year)
                        .Select(
                            x =>
                                new
                                {
                                    Min = x.Min(r => r.CreatedAt),
                                    Max = x.Max(r => r.CreatedAt),
                                }
                        )
                        .FirstOrDefault();

                    YearCB.ItemsSource =
                        minMax != null
                            ? Enumerable.Range(minMax.Min.Year, minMax.Max.Year)
                            : new int[] { DateTime.Now.Year };
                }
            })
        );

        MonthCB.ItemsSource = Enum.GetValues(typeof(Month)).Cast<Month>();

        RefreshDataGrid();
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        CreateStockOutputLogWindow window = new() { Owner = Window.GetWindow(this) };
        window.Closed += (object? sender, EventArgs e) => RefreshDataGrid();
        window.ShowDialog();
    }

    private void FilterButton_Click(object sender, RoutedEventArgs e) => RefreshDataGrid();

    private void RefreshDataGrid()
    {
        Dispatcher.BeginInvoke(
            DispatcherPriority.Background,
            new Action(() =>
            {
                StockOutputLogsDataGrid.ItemsSource = GetData();
            })
        );
    }

    private List<StockOutputLog> GetData()
    {
        var searchText = SearchTB.Text;
        var selectedYear = (int)YearCB.SelectedItem;
        var selectedMonth = (int)MonthCB.SelectedItem;

        using DatabaseContext dbContext = new();

        return dbContext.StockOutputLogs
            .Where(
                x =>
                    (
                        x.Stock!.Item!.Name.ToUpper().Contains(searchText.ToUpper())
                        || x.Stock.Item.Barcode.Contains(searchText)
                    )
                    && x.CreatedAt.Year == selectedYear
                    && selectedMonth != 0
                        ? x.CreatedAt.Month == selectedMonth
                        : true
            )
            .Include(x => x.Stock)
            .ThenInclude(x => x!.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToList();
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        ExportButton.IsEnabled = false;

        Dispatcher.BeginInvoke(
            DispatcherPriority.Background,
            new Action(() =>
            {
                string outputDir = Path.Join("Output", "Pdf");

                var idCi = new CultureInfo("id-ID");
                var now = DateTime.Now.ToString("MMMM yyyy", idCi);
                var name = $"Laporan Barang Keluar ({now}).pdf";

                var savePath = Path.Combine(outputDir, name);

                var data = GetData();
                var pdf = new StockOutputLogExporter(data);

                pdf.Save(savePath);

                Process.Start(new ProcessStartInfo(savePath) { UseShellExecute = true });

                ExportButton.IsEnabled = true;
            })
        );
    }
}
