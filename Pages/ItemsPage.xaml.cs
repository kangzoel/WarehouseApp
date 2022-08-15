using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WarehouseApp.Database;
using WarehouseApp.Models;
using WarehouseApp.Utilities;
using WarehouseApp.Utilities.Pdf;
using WarehouseApp.Windows;


namespace WarehouseApp.Pages;

/// <summary>
/// Interaction logic for ItemsPage.xaml
/// </summary>
public partial class ItemsPage : Page
{
    public ItemsPage() => InitializeComponent();

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        RefreshDataGrid();

        if (Session.Instance.User!.IsAdmin == false)
        {
            AddButton.Visibility = Visibility.Hidden;
        }
    }

    private void RefreshDataGrid()
    {
        EditButton.IsEnabled = false;
        DeleteButton.IsEnabled = false;

        Dispatcher.BeginInvoke(
            DispatcherPriority.Background,
            () =>
            {
                ItemsDataGrid.ItemsSource = GetData();
            }
        );
    }

    private List<Item> GetData()
    {
        var searchText = SearchTB.Text;
        var selectedItemsAvailability = (ComboBoxItem)AvailabilityCB.SelectedValue;
        var availability = (string)selectedItemsAvailability.Content;

        using DatabaseContext dbContext = new();

        var data = dbContext.Items
            .Where(
                item =>
                    (
                        item.Name.ToUpper().Contains(searchText.ToUpper())
                        || item.Barcode.Contains(searchText)
                    )
                    && (
                        availability == "Kosong"
                            ? item.TotalAmount == 0
                            : ( availability == "Tersedia" ? item.TotalAmount > 0 : true )
                    )
            )
            .OrderBy(item => item.Name)
            .ToList();

        return data;
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        CreateEditItemWindow window = new() { Owner = Window.GetWindow(this) };

        window.Closed += (object? sender, EventArgs e) => RefreshDataGrid();
        window.ShowDialog();
    }

    private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var dataGrid = (DataGrid)e.OriginalSource;

        if (dataGrid.SelectedItem is Item)
        {
            EditButton.IsEnabled = true;
            DeleteButton.IsEnabled = true;
        }
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (ItemsDataGrid.SelectedItem is not Item selectedItem)
            return;

        var delete = MessageBox.Show(
            caption: nameof(MessageBoxImage.Warning),
            button: MessageBoxButton.YesNo,
            messageBoxText: $"Apakah Anda yakin ingin menghapus {selectedItem.Name}",
            icon: MessageBoxImage.Warning
        );

        if (delete == MessageBoxResult.Yes)
        {
            Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                () =>
                {
                    using DatabaseContext dbContext = new();

                    var itemToDelete = dbContext.Items.Single(item => item.Id == selectedItem.Id);

                    dbContext.Remove(itemToDelete);

                    dbContext.SaveChanges();

                    MessageBox.Show(
                        caption: "Informasi",
                        button: MessageBoxButton.OK,
                        messageBoxText: $"{selectedItem.Name} berhasil dihapus dari database",
                        icon: MessageBoxImage.Information
                    );

                    RefreshDataGrid();
                }
            );
        }
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (ItemsDataGrid.SelectedItem is Item selectedItem)
        {
            CreateEditItemWindow window =
                new(false, selectedItem) { Owner = Window.GetWindow(this) };
            window.Closed += (object? sender, EventArgs e) => RefreshDataGrid();
            window.ShowDialog();
        }
    }

    private void FilterButton_Click(object sender, RoutedEventArgs e) => RefreshDataGrid();

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        ExportButton.IsEnabled = false;

        Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
        {
            string outputDir = Path.Join("Output", "Pdf");

            var idCi = new CultureInfo("id-ID");
            var now = DateTime.Now.ToString("MMMM yyyy", idCi);
            var name = $"Laporan Daftar Barang ({now}).pdf";

            var savePath = Path.Combine(outputDir, name);

            var data = GetData();
            var pdf = new ItemsExporter(data);

            pdf.Save(savePath);

            Process.Start(new ProcessStartInfo(savePath) { UseShellExecute = true });

            ExportButton.IsEnabled = true;
        }));
    }
}
