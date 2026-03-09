using Daab.Modules.Scientists.Models;
using Daab.Modules.Scientists.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Scientists.Features.Publications.GetByScientistId;

public class GetPublicationsQueryHandler(ScientistsDbContext scientistsDbContext)
    : IRequestHandler<GetPublicationsQuery, GetPublicationsResponse>
{
    public async Task<GetPublicationsResponse> Handle(
        GetPublicationsQuery request,
        CancellationToken cancellationToken
    )
    {
        var publications = await scientistsDbContext
            .Publications.AsNoTracking()
            .Where(x => x.ScientistId == request.ScientistId)
            .ToListAsync(cancellationToken: cancellationToken);
        return new GetPublicationsResponse([
            .. publications.Select(x => new GetPublicationDto(x.Id, x.Title, x.Url, x.ScientistId)),
        ]);
    }
}
