using Microsoft.EntityFrameworkCore;

namespace WarehouseApp.Models;

[Index(nameof(Username), IsUnique = true)]
public class User : BaseModel
{
    public string Username { get; set; }
    public string Name { get; set; }
    public bool IsAdmin { get; set; }
    public string Password { get; set; }

    public User(string username, string name, string password, bool isAdmin)
    {
        Username = username;
        Name = name;
        IsAdmin = isAdmin;
        Password = password;
    }
}
