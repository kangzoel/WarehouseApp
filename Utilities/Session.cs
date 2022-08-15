using WarehouseApp.Models;


namespace WarehouseApp.Utilities;

internal class Session
{
    public User? User { get; set; }

    public static Session Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new();
            }
            return _instance;
        }
    }

    private static Session? _instance;
}
