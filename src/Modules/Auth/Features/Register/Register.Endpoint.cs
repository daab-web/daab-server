using Daab.SharedKernel.Extensions;
using FastEndpoints;
using MediatR;

namespace Daab.Modules.Auth.Features.Register;

public sealed class RegisterEndpoint(IMediator mediator)
    : Endpoint<RegisterRequest, RegisterResponse>
{
    public override void Configure()
    {
        Post("/auth/register");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new RegisterCommand(req), ct);

        await result.Match(
            user => Send.OkAsync(user, ct),
            err => err.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
        );
    }
}
