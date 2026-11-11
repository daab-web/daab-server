using System.Security.Claims;
using Daab.Modules.Auth.Common;
using Daab.Modules.Auth.Options;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ExternalLoginEntity = Daab.Modules.Auth.Models.ExternalLogin;

namespace Daab.Modules.Auth.Features.ExternalLogin;

/// <summary>
/// "Continue with Google" on the sign-in page: sends the browser to Google. Afterwards Google
/// returns to <c>/signin-google</c> (handled by the Google handler), which then redirects to
/// <see cref="GoogleCallbackEndpoint"/>.
/// </summary>
public sealed class GoogleSignInEndpoint(IOptions<GoogleOptions> google) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/auth/external/google");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!google.Value.IsConfigured)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var returnUrl = CookieSession.SafeReturnUrl(Query<string>("returnUrl", isRequired: false));
        var properties = new AuthenticationProperties
        {
            RedirectUri = $"/auth/external/callback?returnUrl={Uri.EscapeDataString(returnUrl)}",
            Items = { [GoogleCallbackEndpoint.ReturnUrlItem] = returnUrl },
        };

        // Sent through FastEndpoints so it knows the response was produced; a bare
        // HttpContext.ChallengeAsync gets its 302 overwritten by the default 204.
        await Send.ResultAsync(Results.Challenge(properties, [ExternalLoginEntity.Google]));
    }
}

/// <summary>
/// Completes Google sign-in: resolves (or creates) the app account, starts the authorization
/// server session and continues where sign-in started (normally the client's /authorize call).
/// </summary>
public sealed class GoogleCallbackEndpoint(
    IMediator mediator,
    ILogger<GoogleCallbackEndpoint> logger
) : EndpointWithoutRequest
{
    public const string ReturnUrlItem = "returnUrl";

    public override void Configure()
    {
        Get("/auth/external/callback");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var returnUrl = CookieSession.SafeReturnUrl(Query<string>("returnUrl", isRequired: false));
        var external = await HttpContext.AuthenticateAsync(CookieSession.ExternalScheme);

        if (!external.Succeeded || external.Principal is null)
        {
            await Send.RedirectAsync(LoginPageUrl(returnUrl, "google"));
            return;
        }

        var principal = external.Principal;
        var command = new ExternalSignInCommand(
            ExternalLoginEntity.Google,
            principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
            principal.FindFirstValue(ClaimTypes.Email),
            string.Equals(
                principal.FindFirstValue(GoogleClaims.EmailVerified),
                "true",
                StringComparison.OrdinalIgnoreCase
            ),
            principal.FindFirstValue(ClaimTypes.Name),
            principal.FindFirstValue(GoogleClaims.Picture)
        );

        await HttpContext.SignOutAsync(CookieSession.ExternalScheme);

        if (string.IsNullOrEmpty(command.ProviderKey))
        {
            await Send.RedirectAsync(LoginPageUrl(returnUrl, "google"));
            return;
        }

        var result = await mediator.Send(command, ct);

        await result.Match(
            async user =>
            {
                await HttpContext.SignInAsync(CookieSession.Scheme, CookieSession.For(user));
                await Send.RedirectAsync(returnUrl);
            },
            async error =>
            {
                logger.LogWarning("Google sign-in refused: {Reason}", error.Message);
                var code = error.Code == 409 ? "google-existing" : "google";
                await Send.RedirectAsync(LoginPageUrl(returnUrl, code));
            }
        );
    }

    public static string LoginPageUrl(string returnUrl, string error) =>
        $"/auth/login?returnUrl={Uri.EscapeDataString(returnUrl)}&error={error}";
}

internal static class GoogleClaims
{
    public const string Picture = "picture";
    public const string EmailVerified = "email_verified";
}
