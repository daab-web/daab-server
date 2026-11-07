namespace Daab.Modules.Auth.Models;

/// <summary>An external identity (e.g. a Google account) that signs in as a <see cref="User"/>.</summary>
public class ExternalLogin
{
    public const string Google = "google";

    public string Id { get; private init; } = Ulid.NewUlid().ToString();

    /// <summary>Provider name, e.g. <see cref="Google"/>.</summary>
    public required string Provider { get; init; }

    /// <summary>The provider's stable id for the account (Google's <c>sub</c>).</summary>
    public required string ProviderKey { get; init; }

    public required string UserId { get; init; }
    public User User { get; init; } = null!;

    public DateTime CreatedAt { get; private init; } = DateTime.UtcNow;
}
