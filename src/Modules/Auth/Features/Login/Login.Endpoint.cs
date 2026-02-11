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
            async t =>
            {
                HttpContext.Response.Cookies.Append("daab.accessToken", t.AccesToken);
                await Send.OkAsync(t, cancellation: ct);
            },
            async err =>
                await Send.ResultAsync(TypedResults.Problem(err.ToProblemDetails(HttpContext)))
        );
    }
}
