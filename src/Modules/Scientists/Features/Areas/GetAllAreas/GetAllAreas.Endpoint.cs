using Daab.Modules.Scientists.Persistence;
using FastEndpoints;
using LanguageExt;
using MediatR;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Scientists.Features.Areas.GetAllAreas;

public sealed record GetAllAreasResponse(string[] Areas);

public sealed class GetAllAreasQuery() : IRequest<GetAllAreasResponse>;

public sealed class GetAllAreasQueryHandler(ScientistsDbContext context)
    : IRequestHandler<GetAllAreasQuery, GetAllAreasResponse>
{
    public Task<GetAllAreasResponse> Handle(
        GetAllAreasQuery request,
        CancellationToken cancellationToken
    )
    {
        return Task.FromResult(
            new GetAllAreasResponse([
                .. context
                    .Scientists.AsNoTracking()
                    .Select(s => s.Areas)
                    .Flatten()
                    .Order()
                    .Distinct(),
            ])
        );
    }
}

public class GetAllAreas(IMediator mediator) : EndpointWithoutRequest<GetAllAreasResponse>
{
    public override void Configure()
    {
        Get("/areas");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var areas = await mediator.Send(new GetAllAreasQuery(), ct);

        await Send.OkAsync(areas, ct);
    }
}
