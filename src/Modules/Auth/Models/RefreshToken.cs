namespace Daab.Modules.Auth.Models;

public class RefreshToken
{
    public string Id { get; init; } = Ulid.NewUlid().ToString();
    public required string Token { get; init; }
    public required string UserId { get; init; }
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; } = DateTimeOffset.UtcNow.AddDays(7);
    public DateTimeOffset? RevokedAt { get; private set; }
    public string? ReplacedByToken { get; private set; }
    public bool IsRevoked { get; private set; }

    public void Revoke()
    {
        IsRevoked = true;
        RevokedAt = DateTimeOffset.UtcNow;
    }

    public void Replace(string newToken) => ReplacedByToken = newToken;

    public User User { get; init; } = null!;
}
