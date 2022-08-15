using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WarehouseApp.Database;
using WarehouseApp.Models;
using WarehouseApp.Utilities;
using WarehouseApp.Windows;

namespace WarehouseApp.Pages;

/// <summary>
/// Interaction logic for UsersPage.xaml
/// </summary>
public partial class UsersPage : Page
{
    public UsersPage()
    {
        InitializeComponent();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        RefreshDataGrid();

        if (Session.Instance.User!.IsAdmin == false)
        {
            ActionsStackPanel.Visibility = Visibility.Hidden;
            ActionsStackPanel.Height = 0;
            ActionStackPanelSpacer.Height = new GridLength(0);
        }
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var window = new CreateEditUserWindow() { Owner = Window.GetWindow(this) };

        window.Closed += (object? o, EventArgs e) => RefreshDataGrid();
        window.ShowDialog();
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        var user = UsersDataGrid.SelectedItem as User;

        MessageBoxResult dialogResult = MessageBox.Show(
            caption: nameof(MessageBoxImage.Question),
            button: MessageBoxButton.YesNo,
            messageBoxText: $"Apakah anda yakin ingin menghapus {user!.Name}?",
            icon: MessageBoxImage.Question
        );

        if (dialogResult == MessageBoxResult.Yes)
        {
            Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() =>
                {
                    using DatabaseContext dbContext = new();

                    dbContext.Remove(user!);

                    MessageBox.Show(
                        caption: "Information",
                        button: MessageBoxButton.OK,
                        messageBoxText: "Pengguna berhasil dihapus",
                        icon: MessageBoxImage.Information
                    );

                    dbContext.SaveChanges();
                })
            );
        }
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (UsersDataGrid.SelectedItem is not User user)
            return;

        CreateEditUserWindow window = new(false, user) { Owner = Window.GetWindow(this) };

        window.Closed += (object? o, EventArgs e) => RefreshDataGrid();
        window.ShowDialog();
    }

    private void RefreshDataGrid()
    {
        EditButton.IsEnabled = false;
        DeleteButton.IsEnabled = false;

        Dispatcher.BeginInvoke(
            DispatcherPriority.Background,
            new Action(() =>
            {
                using DatabaseContext dbContext = new();

                UsersDataGrid.ItemsSource = dbContext.Users
                    .OrderByDescending(x => x.IsAdmin)
                    .ThenBy(user => user.Name)
                    .ToList();
            })
        );
    }

    private void UsersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var btnState = UsersDataGrid.SelectedItem is User;

        EditButton.IsEnabled = btnState;
        DeleteButton.IsEnabled = btnState;
    }

    private void FilterButton_Click(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(
            DispatcherPriority.Background,
            new Action(() =>
            {
                using DatabaseContext dbContext = new();

                UsersDataGrid.ItemsSource = dbContext.Users
                    .Where(
                        user =>
                            user.Name.ToUpper().Contains(NameSearch.Text.ToUpper())
                            || user.Username.ToUpper().Contains(NameSearch.Text.ToUpper())
                    )
                    .OrderByDescending(user => user.IsAdmin)
                    .ThenBy(user => user.Name)
                    .ToList();
            })
        );
    }
}
