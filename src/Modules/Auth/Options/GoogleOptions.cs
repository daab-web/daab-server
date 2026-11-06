namespace Daab.Modules.Auth.Options;

/// <summary>
/// Google OAuth client from the Google Cloud console (APIs &amp; Services → Credentials →
/// OAuth client ID, type "Web application"). Its authorized redirect URI must be
/// <c>{backend public URL}/signin-google</c>. Keep the secret in user-secrets or the environment
/// (<c>Google__ClientSecret</c>). Google sign-in is offered only when both values are set.
/// </summary>
public sealed class GoogleOptions
{
    public const string SectionName = "Google";

    public string? ClientId { get; init; }
    public string? ClientSecret { get; init; }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ClientId) && !string.IsNullOrWhiteSpace(ClientSecret);
}
