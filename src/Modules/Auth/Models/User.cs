namespace Daab.Modules.Auth.Models;

public class User
{
    public string Id { get; private set; }

    public string Username { get; private set; }
    public string PasswordHash { get; private set; }

    public ICollection<Role> Roles { get; private set; } = [];

    public User(string username, string passwordHash)
    {
        Id = Ulid.NewUlid().ToString();

        Username = username;
        PasswordHash = passwordHash;
    }
}
