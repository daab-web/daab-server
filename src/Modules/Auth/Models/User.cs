using Microsoft.AspNetCore.Identity;

namespace Daab.Modules.Auth.Models;

public class User
{
    public string Id { get; }

    public string Username { get; }
    public string PasswordHash { get; init; }

    public ICollection<Role> Roles { get; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; } = [];

    protected User(string username)
    {
        Id = Ulid.NewUlid().ToString();
        Username = username;
        PasswordHash = string.Empty;
    }

    public User(string username, string password)
        : this(username)
    {
        var ph = new PasswordHasher<User>();
        PasswordHash = ph.HashPassword(this, password);
    }
}
