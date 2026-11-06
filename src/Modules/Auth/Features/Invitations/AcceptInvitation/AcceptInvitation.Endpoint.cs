using Daab.Modules.Auth.Common;
using Daab.Modules.Auth.Models;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Daab.Modules.Auth.Features.Invitations.AcceptInvitation;

/// <summary>Shows the "set your password" page an invitation link points to.</summary>
public sealed class AcceptInvitationPageEndpoint(IMediator mediator) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/auth/invitations/{token}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var token = Route<string>("token") ?? string.Empty;
        var result = await mediator.Send(new GetInvitationQuery(token), ct);

        await result.Match(
            user => Html(AcceptInvitationPage.Form(token, user.Email ?? user.Username), ct),
            _ => Html(AcceptInvitationPage.Invalid(), ct, StatusCodes.Status404NotFound)
        );
    }

    private Task Html(string html, CancellationToken ct, int status = StatusCodes.Status200OK) =>
        Send.StringAsync(html, status, contentType: "text/html", cancellation: ct);
}

public sealed record AcceptInvitationRequest
{
    public string Token { get; init; } = string.Empty;
    public string? Password { get; init; }
    public string? ConfirmPassword { get; init; }
}

/// <summary>
/// Activates the invited account: sets its password, signs the user in to the authorization
/// server and offers a link back to the site.
/// </summary>
public sealed class AcceptInvitationEndpoint(IMediator mediator, IConfiguration configuration)
    : Endpoint<AcceptInvitationRequest>
{
    public override void Configure()
    {
        Post("/auth/invitations/{token}");
        AllowAnonymous();
        AllowFormData(urlEncoded: true);
    }

    public override async Task HandleAsync(AcceptInvitationRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(
            new AcceptInvitationCommand(req.Token, req.Password ?? "", req.ConfirmPassword ?? ""),
            ct
        );

        await result.Match(
            async user =>
            {
                await SignInAsync(user);
                await Html(
                    AcceptInvitationPage.Done(user.Email ?? user.Username, ContinueUrl()),
                    ct
                );
            },
            async error =>
            {
                if (error.Code == StatusCodes.Status400BadRequest)
                {
                    // Re-show the form with the problem; the invitation is still usable.
                    var invited = await mediator.Send(new GetInvitationQuery(req.Token), ct);
                    var email = invited.Match(u => u.Email ?? u.Username, _ => string.Empty);
                    await Html(
                        AcceptInvitationPage.Form(req.Token, email, error.Message),
                        ct,
                        StatusCodes.Status400BadRequest
                    );
                    return;
                }

                await Html(AcceptInvitationPage.Invalid(), ct, StatusCodes.Status404NotFound);
            }
        );
    }

    // Signed in to the authorization server, so continuing to the site needs no password again.
    private Task SignInAsync(User user) =>
        HttpContext.SignInAsync(CookieSession.Scheme, CookieSession.For(user));

    /// <summary>
    /// Where "Continue to the site" goes: the client's sign-in route, which completes silently
    /// because the user is now signed in here. <c>Auth:AppUrl</c> is the client's public URL.
    /// </summary>
    private string? ContinueUrl()
    {
        var appUrl = configuration["Auth:AppUrl"]?.TrimEnd('/');
        return string.IsNullOrWhiteSpace(appUrl)
            ? null
            : $"{appUrl}/api/auth/login?redirect={Uri.EscapeDataString("/en/blog")}";
    }

    private Task Html(string html, CancellationToken ct, int status = StatusCodes.Status200OK) =>
        Send.StringAsync(html, status, contentType: "text/html", cancellation: ct);
}
