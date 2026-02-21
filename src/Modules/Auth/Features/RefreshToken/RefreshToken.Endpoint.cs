using FastEndpoints;
using MediatR;

namespace Daab.Modules.Auth.Features.RefreshToken;

public sealed class RefreshTokenEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("/refresh-token");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await Send.OkAsync(ct);
    }
}

public sealed class RefreshTokenCommand : IRequest
{
    public required string RefreshToken { get; init; }
}
