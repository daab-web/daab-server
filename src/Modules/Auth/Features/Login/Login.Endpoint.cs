using FastEndpoints;
using MediatR;

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
        // TODO: Request validation

        var response = await mediator.Send(new LoginCommand(req), ct);

        if (response is null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        await Send.OkAsync(response, ct);
    }
}