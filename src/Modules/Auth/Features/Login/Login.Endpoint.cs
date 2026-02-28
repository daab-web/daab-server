using Daab.SharedKernel.Extensions;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Daab.Modules.Auth.Features.Login;

public sealed class LoginEndpoint(IMediator mediator) : Endpoint<LoginRequest, LoginResponse>
{
    public override void Configure()
    {
        Post("/auth/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var response = await mediator.Send(new LoginCommand(req), ct);

        await response.Match(
            async success =>
            {
                var (loginResponse, refreshToken) = success;
                HttpContext.Response.Cookies.Append(
                    "daab.accessToken",
                    loginResponse.AccessToken,
                    new CookieOptions
                    {
                        HttpOnly = true,
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTimeOffset.UtcNow.AddMinutes(15),
                    }
                );
                HttpContext.Response.Cookies.Append(
                    "daab.refreshToken",
                    refreshToken.Token,
                    new CookieOptions
                    {
                        HttpOnly = true,
                        SameSite = SameSiteMode.Lax,
                        Expires = refreshToken.ExpiresAt,
                    }
                );
                await Send.OkAsync(loginResponse, cancellation: ct);
            },
            async err => await err.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
        );
    }
}
