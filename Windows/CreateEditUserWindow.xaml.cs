using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using WarehouseApp.Database;
using WarehouseApp.Models;

namespace WarehouseApp.Windows;

/// <summary>
/// Interaction logic for CreateUser.xaml
/// </summary>
public partial class CreateEditUserWindow : Window
{
    private readonly bool _create;
    private readonly User? _user;

    public CreateEditUserWindow(bool create = true, User? user = null)
    {
        InitializeComponent();

        _create = create;
        _user = user;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Title = _create ? "Tambah Pengguna" : "Edit Pengguna";

        if (_user != null)
        {
            NameTB.Text = _user.Name;
            Username.Text = _user.Username;
            Role.SelectedIndex = _user.IsAdmin ? 1 : 0;
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        SaveButton.IsEnabled = false;

        Dispatcher.BeginInvoke(
            DispatcherPriority.Background,
            new Action(() =>
            {
                using DatabaseContext dbContext = new();

                var userWithTheSameUsername = dbContext.Users.SingleOrDefault(
                    x =>
                        ( _create && x.Username == Username.Text )
                        || ( !_create && x.Username == Username.Text && x.Id != _user!.Id )
                );
                var successAction = _create ? "ditambahkan" : "diedit";
                var errorMessage = "";

                if (_create) // if 'create' then password is required
                {
                    if (!Password.Password.Equals(PasswordConfirm.Password))
                        errorMessage = "Konfirmasi Password tidak sesuai";

                    if (Password.Password.Length < 8 || Password.Password.Length > 255)
                        errorMessage = "Password harus memiliki panjang 5-255 karakter";
                }
                else if (Password.Password.Length != 0) // if 'update' & password length != 0
                {
                    if (!Password.Password.Equals(PasswordConfirm.Password))
                        errorMessage = "Konfirmasi Password tidak sesuai";
                }

                if (Username.Text.Length < 4 || Username.Text.Length > 255)
                    errorMessage = "Username harus memiliki panjang 5-255 karakter";

                if (NameTB.Text.Length < 5 || NameTB.Text.Length > 255)
                    errorMessage = "Nama harus memiliki panjang 5-255 karakter";

                if (userWithTheSameUsername != null)
                    errorMessage = "Username sudah dipakai. Gunakan username lain!";

                if (errorMessage.Length != 0)
                {
                    MessageBox.Show(
                        caption: nameof(MessageBoxImage.Error),
                        button: MessageBoxButton.OK,
                        messageBoxText: errorMessage,
                        icon: MessageBoxImage.Error
                    );

                    SaveButton.IsEnabled = true;

                    return;
                }

                if (_create)
                {
                    dbContext.Users.Add(
                        new User(
                            name: NameTB.Text,
                            username: Username.Text,
                            isAdmin: Role.SelectedValue.Equals("Admin"),
                            password: Password.Password
                        )
                    );
                }
                else
                {
                    var user = dbContext.Users.Single(x => x.Id == _user!.Id);

                    user.Name = NameTB.Text;
                    user.Username = Username.Text;
                    user.IsAdmin = Role.SelectedValue.Equals("Admin");
                    user.Password =
                        Password.Password.Length > 0 ? Password.Password : user.Password;
                }

                dbContext.SaveChanges();

                MessageBox.Show(
                    caption: "Information",
                    button: MessageBoxButton.OK,
                    messageBoxText: $"Pengguna berhasil {successAction}",
                    icon: MessageBoxImage.Information
                );

                Close();
            })
        );
    }
}
