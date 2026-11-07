using FastEndpoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;

namespace Daab.Modules.Auth.Features.Logout;

/// <summary>
/// Ends the authorization-server sign-in session (the "cookie" scheme), so the next sign-in
/// asks for credentials again instead of silently reusing the previous account. Redirects back
/// only to an origin listed in <c>Cors:AllowedOrigins</c>.
/// </summary>
public sealed class LogoutEndpoint(IConfiguration configuration) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/auth/logout");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await HttpContext.SignOutAsync("cookie");

        var returnUrl = Query<string>("returnUrl", isRequired: false);
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        var target =
            Uri.TryCreate(returnUrl, UriKind.Absolute, out var uri)
            && allowedOrigins.Contains(
                uri.GetLeftPart(UriPartial.Authority),
                StringComparer.OrdinalIgnoreCase
            )
                ? uri.ToString()
                : "/auth/login";

        await Send.RedirectAsync(target, allowRemoteRedirects: true);
    }
}
