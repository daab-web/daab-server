using Microsoft.AspNetCore.Identity;

namespace Daab.Modules.Auth.Models;

public class User
{
    public string Id { get; }

    public string Username { get; }
    public string PasswordHash { get; private set; }

    public string? Email { get; private set; }
    public string? FullName { get; private set; }
    public string? PictureUrl { get; private set; }
    public DateTime CreatedAt { get; private init; } = DateTime.UtcNow;

    public ICollection<Role> Roles { get; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; } = [];
    public ICollection<ExternalLogin> ExternalLogins { get; } = [];

    /// <summary>Name shown publicly (e.g. as a comment or post author).</summary>
    public string DisplayName => string.IsNullOrWhiteSpace(FullName) ? Username : FullName;

    /// <summary>
    /// An invited user has no password until they accept the invitation, and cannot sign in
    /// before that.
    /// </summary>
    public bool HasPassword => !string.IsNullOrEmpty(PasswordHash);

    protected User(string username)
    {
        Id = Ulid.NewUlid().ToString();
        Username = username;
        PasswordHash = string.Empty;
    }

    public User(string username, string password)
        : this(username)
    {
        SetPassword(password);
    }

    /// <summary>Self-registered account; the e-mail doubles as the login username.</summary>
    public static User Register(string email, string fullName, string password)
    {
        var normalizedEmail = NormalizeEmail(email);

        return new User(normalizedEmail, password)
        {
            Email = normalizedEmail,
            FullName = fullName.Trim(),
        };
    }

    /// <summary>
    /// Account created by an administrator's invitation. It has no password until the invited
    /// person accepts the invitation; the e-mail doubles as the login username.
    /// </summary>
    public static User Invite(string email, string fullName)
    {
        var normalizedEmail = NormalizeEmail(email);

        return new User(normalizedEmail) { Email = normalizedEmail, FullName = fullName.Trim() };
    }

    /// <summary>
    /// Account created on first sign-in with an external provider (e.g. Google), from the
    /// profile the provider returned. It has no password; the user signs in through the provider.
    /// </summary>
    public static User FromExternal(string email, string? fullName, string? pictureUrl)
    {
        var normalizedEmail = NormalizeEmail(email);

        return new User(normalizedEmail)
        {
            Email = normalizedEmail,
            FullName = string.IsNullOrWhiteSpace(fullName) ? null : fullName.Trim(),
            PictureUrl = pictureUrl,
        };
    }

    /// <summary>Fills in profile details an external provider knows and the account lacks.</summary>
    public void FillProfileFrom(string? fullName, string? pictureUrl)
    {
        if (string.IsNullOrWhiteSpace(FullName) && !string.IsNullOrWhiteSpace(fullName))
        {
            FullName = fullName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(pictureUrl))
        {
            PictureUrl = pictureUrl;
        }
    }

    public void SetPassword(string password)
    {
        PasswordHash = new PasswordHasher<User>().HashPassword(this, password);
    }

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
