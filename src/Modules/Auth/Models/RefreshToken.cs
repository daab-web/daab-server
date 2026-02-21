namespace Daab.Modules.Auth.Models;

public class RefreshToken
{
    public string Id { get; init; } = Ulid.NewUlid().ToString();
    public required string Token { get; init; }
    public required string UserId { get; init; }
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; } = DateTimeOffset.UtcNow.AddDays(7);
    public string? ReplacedByToken { get; private set; }
    public bool IsRevoked { get; private set; }

    public void Revoke() => IsRevoked = true;

    public User Users { get; init; } = null!;
}
