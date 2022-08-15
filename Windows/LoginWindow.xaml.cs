using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using WarehouseApp.Database;
using WarehouseApp.Models;
using WarehouseApp.Utilities;
using BcryptNet = BCrypt.Net.BCrypt;

namespace WarehouseApp.Windows;

/// <summary>
/// Interaction logic for Login.xaml
/// </summary>
public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
    }

    private void LogInButton_Click(object sender, RoutedEventArgs e)
    {
        LogInButton.IsEnabled = false;

        Dispatcher.BeginInvoke(
            DispatcherPriority.Background,
            () =>
            {
                using DatabaseContext dbContext = new();

                User? account = dbContext.Users.FirstOrDefault(x => x.Username == Username.Text);

                if (account != null)
                {
                    if (BcryptNet.Verify(Password.Password, account.Password))
                    {
                        Session.Instance.User = account;

                        Topmost = false;
                        Deactivated -= Window_Deactivated;
                        Hide();

                        MainWindow window = new() { Owner = this };
                        window.Show();

                        return;
                    }
                }

                LogInButton.IsEnabled = true;

                MessageBox.Show(
                    caption: "Error",
                    button: MessageBoxButton.OK,
                    messageBoxText: "Pastikan username & password benar!",
                    icon: MessageBoxImage.Error
                );
            }
        );
    }

    private void Window_Deactivated(object? sender, EventArgs e)
    {
        Topmost = true;
    }
}
