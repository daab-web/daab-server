namespace Daab.Modules.Auth.Models;

public class RefreshToken
{
    public string Id { get; init; } = Ulid.NewUlid().ToString();
    public required string Token { get; init; }
    public required string UserId { get; init; }
    public DateTime CreatedAt { get; private init; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; init; } = DateTime.UtcNow.AddDays(7);
    public DateTime? RevokedAt { get; private set; }
    public string? ReplacedByToken { get; private set; }
    public bool IsRevoked { get; private set; }

    public void Revoke()
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
    }

    public void Replace(string newToken) => ReplacedByToken = newToken;

    public User User { get; init; } = null!;
}
