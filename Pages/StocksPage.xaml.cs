using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.EntityFrameworkCore;
using WarehouseApp.Database;
using WarehouseApp.Models;
using WarehouseApp.Utilities;
using WarehouseApp.Windows;

namespace WarehouseApp.Pages;

/// <summary>
/// Interaction logic for StocksPage.xaml
/// </summary>
public partial class StocksPage : Page
{
    public StocksPage()
    {
        InitializeComponent();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        RefreshDataGrid();

        if (Session.Instance.User!.IsAdmin == false)
        {
            ActionsSP.Visibility = Visibility.Collapsed;
            AddActionSP.Visibility = Visibility.Collapsed;
        }
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedItem = (Stock)StocksDataGrid.SelectedItem;

        Dispatcher.BeginInvoke(
            DispatcherPriority.Background,
            () =>
            {
                var formattedDate = selectedItem.ExpirationDate.ToString(
                    "d",
                    CultureInfo.GetCultureInfo("id-ID")
                );

                var delete = MessageBox.Show(
                    caption: nameof(MessageBoxImage.Warning),
                    button: MessageBoxButton.YesNo,
                    messageBoxText: $"Apakah Anda yakin ingin menghapus stok\n({selectedItem.Item!.Name}, {formattedDate})",
                    icon: MessageBoxImage.Warning
                );

                if (delete == MessageBoxResult.Yes)
                {
                    using DatabaseContext dbContext = new();

                    var item = dbContext.Items.Find(selectedItem.Item.Id);

                    item!.TotalAmount -= selectedItem.Amount;

                    selectedItem.Item = null;

                    dbContext.Remove(selectedItem);

                    dbContext.SaveChanges();

                    MessageBox.Show(
                        caption: nameof(MessageBoxImage.Information),
                        button: MessageBoxButton.OK,
                        messageBoxText: $"Stok berhasil dihapus",
                        icon: MessageBoxImage.Information
                    );

                    RefreshDataGrid();
                }
            }
        );
    }

    private void StocksDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        DeleteButton.IsEnabled = StocksDataGrid.SelectedItem is Stock;
    }

    private void FilterButton_Click(object sender, RoutedEventArgs e) => RefreshDataGrid();

    private void RefreshDataGrid()
    {
        DeleteButton.IsEnabled = false;

        Dispatcher.BeginInvoke(
            DispatcherPriority.Background,
            () =>
            {
                var selectedAvailabilityCBI = (ComboBoxItem)AvailabilityCB.SelectedValue;
                var selectedExpirationCBI = (ComboBoxItem)ExpirationCB.SelectedValue;
                var selectedAvailability = (string)selectedAvailabilityCBI.Content;
                var selectedExpiration = (string)selectedExpirationCBI.Content;
                var searchText = SearchTB.Text;

                using DatabaseContext dbContext = new();

                StocksDataGrid.ItemsSource = dbContext.Stocks
                    .Where(
                        x =>
                            (
                                x.Item!.Name.ToUpper().Contains(searchText.ToUpper())
                                || x.Item.Barcode.Contains(searchText)
                            )
                            && ( selectedAvailability == "Kosong" ? x.Amount == 0 : true )
                            && ( selectedAvailability == "Tersedia" ? x.Amount != 0 : true )
                            && (
                                selectedExpiration == "Belum Kedaluwarsa"
                                    ? x.ExpirationDate > DateTime.Now
                                    : (
                                        selectedExpiration != "Kedaluwarsa"
                                        || x.ExpirationDate <= DateTime.Now
                                    )
                            )
                    )
                    .Include(stock => stock.Item)
                    .OrderBy(stock => stock.Item!.Name)
                    .ThenBy(stock => stock.ExpirationDate)
                    .ToList();
            }
        );
    }

    private void StockInputLogAddButton_Click(object sender, RoutedEventArgs e)
    {
        var window = new CreateStockInputLogWindow() { Owner = Window.GetWindow(this) };

        window.Closed += (object? sender, EventArgs e) => RefreshDataGrid();
        window.ShowDialog();
    }

    private void StockOutputLogAddButton_Click(object sender, RoutedEventArgs e)
    {
        var window = new CreateStockOutputLogWindow() { Owner = Window.GetWindow(this) };

        window.Closed += (object? sender, EventArgs e) => RefreshDataGrid();
        window.ShowDialog();
    }
}
