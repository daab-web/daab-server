using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Daab.SharedKernel.Extensions;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

namespace Daab.Modules.Auth.Features.Login;

public sealed record OAuthLoginRequest
{
    public required string Username { get; init; }
    public required string Password { get; init; }

    [QueryParam]
    public required string ReturnUrl { get; init; }
}

public sealed class OAuthLoginEndpoint(IMediator mediator, ILogger<OAuthLoginEndpoint> logger)
    : Endpoint<OAuthLoginRequest>
{
    public override void Configure()
    {
        Post("/auth/login");
        AllowAnonymous();
        AllowFormData(urlEncoded: true);
    }

    public override async Task HandleAsync(OAuthLoginRequest req, CancellationToken ct)
    {
        logger.LogDebug("Login request: {req}", req);
        var loginRequest = new LoginRequest(req.Username, req.Password);
        var response = await mediator.Send(new LoginCommand(loginRequest), ct);

        await response.Match(
            async user =>
            {
                await HttpContext.SignInAsync(
                    "cookie",
                    new ClaimsPrincipal(
                        new ClaimsIdentity(
                            [
                                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                                new Claim(JwtRegisteredClaimNames.Name, user.Username),
                                new Claim(JwtRegisteredClaimNames.Email, "admin@waas.org"),
                                new Claim(
                                    "roles",
                                    string.Join(',', user.Roles.Select(r => r.Name))
                                ),
                            ],
                            "cookie"
                        )
                    )
                );

                await Send.RedirectAsync(req.ReturnUrl);
            },
            async err => await err.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
        );
    }
}
