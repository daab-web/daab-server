using Daab.SharedKernel;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http.Extensions;
using ZiggyCreatures.Caching.Fusion;

namespace Daab.Modules.Scientists.Features.GetAllScientists;

public sealed class GetAllScientistsEndpoint(IMediator mediator, IFusionCache cache)
    : Endpoint<GetAllScientistsRequest, PagedResponse<GetAllScientistsResponse>>
{
    public override void Configure()
    {
        Get("/scientists");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetAllScientistsRequest request, CancellationToken ct)
    {
        var response = await cache.GetOrSetAsync(
            HttpContext.Request.GetEncodedUrl(),
            async cancellationToken =>
                await mediator.Send(new GetAllScientistsQuery(request), cancellationToken),
            token: ct
        );

        await Send.OkAsync(response, ct);
    }
}
