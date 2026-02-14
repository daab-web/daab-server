using Daab.Modules.Scientists.Persistence;
using FastEndpoints;
using LanguageExt;
using MediatR;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.EntityFrameworkCore;
using ZiggyCreatures.Caching.Fusion;

namespace Daab.Modules.Scientists.Features.Areas.GetAllAreas;

public sealed record GetAllAreasResponse(string[] Areas);

public sealed class GetAllAreasQuery() : IRequest<GetAllAreasResponse>;

public sealed class GetAllAreasQueryHandler(ScientistsDbContext context)
    : IRequestHandler<GetAllAreasQuery, GetAllAreasResponse>
{
    public async Task<GetAllAreasResponse> Handle(
        GetAllAreasQuery request,
        CancellationToken cancellationToken
    )
    {
        return new GetAllAreasResponse([
            .. context.Scientists.AsNoTracking().Select(s => s.Countries).Flatten().Distinct(),
        ]);
    }
}

public class GetAllAreas(IMediator mediator, IFusionCache cache)
    : EndpointWithoutRequest<GetAllAreasResponse>
{
    public override void Configure()
    {
        Get("/areas");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var areas = await cache.GetOrSetAsync(
            HttpContext.Request.GetEncodedUrl(),
            async cancellationToken =>
                await mediator.Send(new GetAllAreasQuery(), cancellationToken),
            token: ct
        );

        await Send.OkAsync(areas, ct);
    }
}
