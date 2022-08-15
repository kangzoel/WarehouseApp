using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WarehouseApp.Database;
using WarehouseApp.Utilities;
using BCryptNet = BCrypt.Net.BCrypt;

namespace WarehouseApp.Pages;
/// <summary>
/// Interaction logic for AccountSettingsPage.xaml
/// </summary>
public partial class AccountSettingsPage : Page
{
    public AccountSettingsPage()
    {
        InitializeComponent();
    }

    private void ChangePassButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var errorMessage = "";

        if (NewPasswordPB.Password != ConfirmationPasswordPB.Password)
            errorMessage = "Konfirmasi Password tidak sama dengan Password Baru";

        if (!BCryptNet.Verify(OldPasswordPB.Password, Session.Instance.User!.Password))
            errorMessage = "Password Sekarang tidak valid";

        if (errorMessage.Length != 0)
        {
            MessageBox.Show(
                caption: nameof(MessageBoxImage.Error),
                button: MessageBoxButton.OK,
                messageBoxText: errorMessage,
                icon: MessageBoxImage.Error
            );

            return;
        }

        using DatabaseContext dbContext = new();

        var user = dbContext.Users.Where(user => user.Id == Session.Instance.User.Id).Single();

        user.Password = NewPasswordPB.Password;

        dbContext.SaveChanges();

        OldPasswordPB.Password = "";
        NewPasswordPB.Password = "";
        ConfirmationPasswordPB.Password = "";

        MessageBox.Show(
            caption: nameof(MessageBoxImage.Information),
            button: MessageBoxButton.OK,
            messageBoxText: "Password Anda berhasil diubah",
            icon: MessageBoxImage.Information
        );
    }
}
