using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using WarehouseApp.Database;
using WarehouseApp.Utilities;
using WarehouseApp.Windows;

namespace WarehouseApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private async void Application_Startup(object sender, StartupEventArgs e)
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("id-ID");

        using DatabaseContext dbContext = new();
        await dbContext.Database.EnsureCreatedAsync();

        // if users is empty, seed the data
        if (dbContext.Users.FirstOrDefault() == null)
            Seeder.Seed();

#if DEBUG
        MainWindow window = new();
        Session.Instance.User = dbContext.Users?.First(x => x.IsAdmin);
#else
        LoginWindow window = new();
#endif

        window.Show();
    }
}
