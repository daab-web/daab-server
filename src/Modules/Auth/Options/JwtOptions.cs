namespace Daab.Modules.Auth.Options;

public sealed class JwtOptions
{
    public required string JwtSecret { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public int ExpiresMinutes { get; init; }
}