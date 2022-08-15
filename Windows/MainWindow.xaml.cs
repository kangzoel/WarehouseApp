using System;
using System.Windows;
using System.Windows.Controls;
using WarehouseApp.Models;
using WarehouseApp.Utilities;

namespace WarehouseApp.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (Session.Instance.User is User user)
            Title = $"{Title} | {user.Name}";

        if (Session.Instance.User!.IsAdmin == false)
            UsersLBI.Visibility = Visibility.Collapsed;

        var selectedMenu = (ListBoxItem)Menu_LB.SelectedItem;

        selectedMenu.Focus();
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        if (Owner != null)
            Owner.Close();
    }

    private void Menu_LB_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TheFrame is not Frame frame)
            return;

        var listBox = (ListBox)e.OriginalSource;

        switch (listBox.SelectedIndex)
        {
            case 0: // Data Barang
                frame.Navigate(new Uri("/Pages/ItemsPage.xaml", UriKind.Relative));
                break;
            case 1: // Stok
                frame.Navigate(new Uri("/Pages/StocksPage.xaml", UriKind.Relative));
                break;
            case 2: // Barang Masuk
                frame.Navigate(new Uri("/Pages/StockInputLogsPage.xaml", UriKind.Relative));
                break;
            case 3: // Barang Keluar
                frame.Navigate(new Uri("/Pages/StockOutputLogsPage.xaml", UriKind.Relative));
                break;
            case 4: // Daftar Pengguna
                frame.Navigate(new Uri("/Pages/UsersPage.xaml", UriKind.Relative));
                break;
            case 5: // Pengaturan Akun
                frame.Navigate(new Uri("/Pages/AccountSettingsPage.xaml", UriKind.Relative));
                break;
        }
    }
}
