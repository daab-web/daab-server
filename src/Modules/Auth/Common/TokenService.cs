using System.Security.Cryptography;

namespace Daab.Modules.Auth.Common;

public class RefreshToken
{
    public required string Token { get; init; }
    public required string UserId { get; init; }
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; } = DateTimeOffset.UtcNow.AddDays(7);
    public bool IsRevoked { get; private set; }

    public void Revoke() => IsRevoked = true;
}

public static class TokenService
{
    public static RefreshToken GenerateRefreshToken(string userId)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        return new RefreshToken { Token = token, UserId = userId };
    }
}
