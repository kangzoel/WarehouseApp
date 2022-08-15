using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WarehouseApp.Models;
using WarehouseApp.Utilities;

namespace WarehouseApp.Windows;

/// <summary>
/// Interaction logic for OutputItemInputWindow.xaml
/// </summary>
public partial class OutputItemInputWindow : Window
{
    private readonly DataGrid _stocksDataGrid;
    private readonly DataGrid _stockOutputLogsDataGrid;
    private readonly int _stockIndex;
    private readonly Stock _stockItem;
    private List<StockOutputLog> _stockOutputLogList;

    public OutputItemInputWindow(
        DataGrid stocksDataGrid,
        DataGrid stockOutputLogsDataGrid,
        int stockIndex,
        Stock stockItem,
        List<StockOutputLog> stockOutputLogList
    )
    {
        InitializeComponent();

        _stocksDataGrid = stocksDataGrid;
        _stockOutputLogsDataGrid = stockOutputLogsDataGrid;
        _stockIndex = stockIndex;
        _stockItem = stockItem;
        _stockOutputLogList = stockOutputLogList;
    }

    private void OKButton_Click(object sender, RoutedEventArgs e)
    {
        OKButton.IsEnabled = false;

        bool amountIsNumber = int.TryParse(AmountTB.Text, out int outputAmount);
        var errorMessage = "";

        if (amountIsNumber == false)
            errorMessage = "Jumlah barang tidak valid";
        else if (outputAmount < 1)
            errorMessage = "Jumlah barang minimal adalah 1";
        else if (outputAmount > _stockItem.Amount)
            errorMessage = $"Jumlah barang tidak boleh melebihi jumlah stok ({_stockItem.Amount})";

        if (AmountTB.Text == "")
            errorMessage = "Harap masukkan jumlah barang!";

        if (errorMessage != "")
        {
            MessageBox.Show(
                caption: nameof(MessageBoxImage.Error),
                button: MessageBoxButton.OK,
                messageBoxText: errorMessage,
                icon: MessageBoxImage.Error
            );

            OKButton.IsEnabled = true;

            return;
        }

        ( _stocksDataGrid.Items[_stockIndex] as Stock )!.Amount -= outputAmount;

        _stocksDataGrid.Items.Refresh();

        var existingOutput = _stockOutputLogList.SingleOrDefault(x => x.Stock!.Id == _stockItem.Id);

        // if stockoutputlogs with the current id is exist in the database, then increment the amount
        if (existingOutput is StockOutputLog)
        {
            existingOutput.Amount += outputAmount;
            _stockOutputLogsDataGrid.Items.Refresh();
        }
        else // create new stockoutputlogs record
        {
            _stockOutputLogsDataGrid.ItemsSource = _stockOutputLogList;

            using Database.DatabaseContext dbContext = new();

            _stockOutputLogList.Add(
                new StockOutputLog()
                {
                    StockId = _stockItem.Id,
                    UserId = Session.Instance.User!.Id,
                    Amount = outputAmount,
                    Stock = _stockItem
                }
            );
            _stockOutputLogsDataGrid.Items.Refresh();
        }

        Close();
    }
}
