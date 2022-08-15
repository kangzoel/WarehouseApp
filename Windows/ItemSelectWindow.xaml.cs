using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using WarehouseApp.Database;
using WarehouseApp.Models;

namespace WarehouseApp.Windows;

/// <summary>
/// Interaction logic for ItemSelectWindow.xaml
/// </summary>
public partial class ItemSelectWindow : Window
{
    public ItemSelectWindow()
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

                var items = dbContext.Items.OrderBy(x => x.Name).ToList();

                ItemsDataGrid.ItemsSource = items;

                Style rowStyle = new(typeof(DataGridRow));
                rowStyle.Setters.Add(
                    new EventSetter(
                        MouseDoubleClickEvent,
                        new MouseButtonEventHandler(Row_DoubleClick)
                    )
                );
                ItemsDataGrid.RowStyle = rowStyle;
            }
        );
    }

    private void ItemsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ItemsDataGrid.SelectedItem != null)
            ChooseButton.IsEnabled = true;
        else
            ChooseButton.IsEnabled = false;
    }

    private void Row_DoubleClick(object sender, MouseButtonEventArgs e) => ChooseSelectedItem();

    private void ChooseButton_Click(object sender, RoutedEventArgs e) => ChooseSelectedItem();

    private void ChooseSelectedItem()
    {
        Item? selectedItem = ItemsDataGrid.SelectedItem as Item;
        CreateStockInputLogWindow? owner = Owner as CreateStockInputLogWindow;

        owner!.Item = selectedItem!;
        owner!.Barcode.Text = selectedItem!.Barcode;
        owner!.Quantifier.Text = selectedItem!.Quantifier;
        owner!.ItemName.Text = selectedItem!.Name;

        Close();
    }
}
