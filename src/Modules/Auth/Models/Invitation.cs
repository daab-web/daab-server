using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Daab.Modules.Auth.Models;

/// <summary>
/// One-time link that lets an invited user set their password. Only a hash of the token is
/// stored; the raw token exists only in the link handed to the administrator.
/// </summary>
public class Invitation
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromDays(7);

    public string Id { get; private init; } = Ulid.NewUlid().ToString();
    public required string UserId { get; init; }
    public User User { get; init; } = null!;

    public required string TokenHash { get; init; }

    public DateTime CreatedAt { get; private init; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; private init; } = DateTime.UtcNow.Add(Lifetime);
    public DateTime? AcceptedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    public bool IsUsable(DateTime now) =>
        AcceptedAt is null && RevokedAt is null && ExpiresAt > now;

    public void Accept() => AcceptedAt = DateTime.UtcNow;

    public void Revoke() => RevokedAt ??= DateTime.UtcNow;

    /// <summary>Creates an invitation and returns it together with its raw token.</summary>
    public static (Invitation Invitation, string Token) Issue(string userId)
    {
        var token = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32));
        return (new Invitation { UserId = userId, TokenHash = Hash(token) }, token);
    }

    public static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
