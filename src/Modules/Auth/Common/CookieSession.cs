using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Daab.Modules.Auth.Models;

namespace Daab.Modules.Auth.Common;

/// <summary>The authorization server's own sign-in session (the <c>cookie</c> scheme).</summary>
internal static class CookieSession
{
    public const string Scheme = "cookie";

    /// <summary>Short-lived session holding the provider's identity during external sign-in.</summary>
    public const string ExternalScheme = "external";

    /// <summary>Same claims as the password sign-in form issues.</summary>
    public static ClaimsPrincipal For(User user) =>
        new(
            new ClaimsIdentity(
                [
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                    new Claim(JwtRegisteredClaimNames.Name, user.DisplayName),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                    new Claim(AuthClaims.Roles, string.Join(',', user.Roles.Select(r => r.Name))),
                ],
                Scheme
            )
        );

    /// <summary>
    /// Only same-site paths may be returned to after sign-in (prevents open redirects).
    /// </summary>
    public static string SafeReturnUrl(string? returnUrl) =>
        !string.IsNullOrEmpty(returnUrl)
        && returnUrl.StartsWith('/')
        && !returnUrl.StartsWith("//", StringComparison.Ordinal)
        && !returnUrl.StartsWith("/\\", StringComparison.Ordinal)
            ? returnUrl
            : "/";
}
