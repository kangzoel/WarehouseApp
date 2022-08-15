using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using WarehouseApp.Database;
using WarehouseApp.Models;
using WarehouseApp.Utilities;

namespace WarehouseApp.Windows;

/// <summary>
/// Interaction logic for CreateItemWindow.xaml
/// </summary>
public partial class CreateStockInputLogWindow : Window
{
    public Item? Item { get; set; }

    public CreateStockInputLogWindow()
    {
        InitializeComponent();
    }

    private void ChooseItem_Click(object sender, RoutedEventArgs e)
    {
        ItemSelectWindow w = new() { Owner = this };
        w.ShowDialog();
    }

    private void Expirable_Changed(object sender, RoutedEventArgs e)
    {
        if (Expirable.IsChecked == null)
            return;

        if ((bool)Expirable.IsChecked)
        {
            ExpirationDate.SelectedDate = DateTime.MaxValue;
            ExpirationDate.IsEnabled = false;
        }
        else
        {
            ExpirationDate.SelectedDate = DateTime.Now; // reset default selected date when opening the date picker
            ExpirationDate.SelectedDate = null;
            ExpirationDate.IsEnabled = true;
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        SaveButton.IsEnabled = false;

        var errorMessage = "";

        if (Amount.Text.Length == 0)
            errorMessage = "Harap isi jumlah barang!";

        if (ExpirationDate.SelectedDate == null)
            errorMessage = "Harap isi tanggal kedaluwarsa!";

        if (Item == null)
            errorMessage = "Harap pilih barang terlebih dahulu!";

        if (errorMessage.Length != 0)
        {
            MessageBox.Show(
                caption: "Error",
                button: MessageBoxButton.OK,
                messageBoxText: errorMessage,
                icon: MessageBoxImage.Error
            );

            SaveButton.IsEnabled = true;

            return;
        }

        Dispatcher.BeginInvoke(
            DispatcherPriority.Background,
            () =>
            {
                using DatabaseContext dbContext = new();

                var item = dbContext.Items.Where(i => i.Id == Item!.Id).SingleOrDefault();
                var expirationDate = (DateTime)ExpirationDate.SelectedDate!;
                var stock = dbContext.Stocks.SingleOrDefault(
                    stock => ( stock.ItemId == Item!.Id ) && ( stock.ExpirationDate == expirationDate )
                );

                int inputAmount = Convert.ToInt32(Amount.Text);

                if (stock == null)
                {
                    dbContext.Add(
                        new StockInputLog()
                        {
                            UserId = Session.Instance.User!.Id,
                            Amount = inputAmount,
                            Stock = new Stock()
                            {
                                ExpirationDate = expirationDate,
                                ItemId = Item!.Id,
                            }
                        }
                    );
                }
                else
                {
                    dbContext.Add(
                        new StockInputLog()
                        {
                            StockId = stock.Id,
                            UserId = Session.Instance.User!.Id,
                            Amount = inputAmount,
                        }
                    );
                }

                dbContext.SaveChanges();

                MessageBox.Show(
                    caption: "Sukses",
                    button: MessageBoxButton.OK,
                    messageBoxText: "Barang masuk berhasil ditambahkan ke stok",
                    icon: MessageBoxImage.Information
                );

                Close();
            }
        );
    }
}
