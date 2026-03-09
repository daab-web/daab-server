using System.Security.Claims;
using Daab.Modules.Auth.Common;
using Daab.SharedKernel.Extensions;
using FastEndpoints;
using FastEndpoints.Security;
using MediatR;

namespace Daab.Modules.Auth.Features.Login;

public sealed class LoginEndpoint(IMediator mediator) : Endpoint<LoginRequest, TokenResponse>
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
            async user =>
            {
                var tokenResponse = await CreateTokenWith<TokenService>(
                    user.Id,
                    p =>
                    {
                        p.Claims.AddRange([
                            new Claim(ClaimTypes.NameIdentifier, user.Id),
                            new Claim(ClaimTypes.Name, user.Username),
                        ]);
                        p.Roles.AddRange(user.Roles.Select(r => r.Name));
                    }
                );

                HttpContext.SetAuthCookies(tokenResponse);

                await Send.OkAsync(tokenResponse, cancellation: ct);
            },
            async err => await err.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
        );
    }
}
