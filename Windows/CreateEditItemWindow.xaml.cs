using System.Linq;
using System.Windows;
using System.Windows.Threading;
using WarehouseApp.Database;
using WarehouseApp.Models;

namespace WarehouseApp.Windows;

/// <summary>
/// Interaction logic for CreateEditItemWindow.xaml
/// </summary>
public partial class CreateEditItemWindow : Window
{
    private readonly Item? _item;
    private readonly bool _create;

    public CreateEditItemWindow(bool create = true, Item? item = null)
    {
        InitializeComponent();

        _create = create;
        _item = item;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Title = _create ? "Tambah Barang" : "Edit Barang";

        if (_item != null)
        {
            BarcodeTB.Text = _item!.Barcode;
            NameTB.Text = _item.Name;
            QuantifierTB.Text = _item.Quantifier;
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        SaveButton.IsEnabled = false;

        Dispatcher.BeginInvoke(
            DispatcherPriority.Background,
            () =>
            {
                using DatabaseContext dbContext = new();

                var itemWithTheSameBarcode = dbContext.Items.SingleOrDefault(
                    i =>
                        ( _create && i.Barcode == BarcodeTB.Text )
                        || ( !_create && i.Barcode == BarcodeTB.Text && i.Id != _item!.Id )
                );
                var successAction = _create ? "ditambahkan" : "diedit";
                var errorMessage = "";

                if (BarcodeTB.Text.Length != 13)
                    errorMessage = "Barcode harus memiliki 13 angka";

                if (NameTB.Text.Length == 0)
                    errorMessage = "Nama barang harus diisi";

                if (QuantifierTB.Text.Length == 0)
                    errorMessage = "Satuan barang harus diisi";

                if (itemWithTheSameBarcode != null)
                    errorMessage = "Barcode sudah digunakan oleh barang lain";

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

                if (_create)
                {
                    dbContext.Items.Add(
                        new Item(
                            barcode: BarcodeTB.Text,
                            name: NameTB.Text,
                            quantifier: QuantifierTB.Text
                        )
                    );
                }
                else
                {
                    Item item = dbContext.Items.Single(i => i.Id == _item!.Id);

                    item.Barcode = BarcodeTB.Text;
                    item.Name = NameTB.Text;
                    item.Quantifier = QuantifierTB.Text;
                }

                dbContext.SaveChanges();

                MessageBox.Show(
                    caption: "Information",
                    button: MessageBoxButton.OK,
                    messageBoxText: $"Barang berhasil {successAction}",
                    icon: MessageBoxImage.Information
                );

                Close();
            }
        );
    }
}
