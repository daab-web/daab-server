using Daab.Modules.Scientists.Models;
using Daab.Modules.Scientists.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Scientists.Features.GetScientist;

public class GetScientistQueryHandler(ScientistsContext context)
    : IRequestHandler<GetScientistQuery, GetScientistResponse?>
{
    public async Task<GetScientistResponse?> Handle(
        GetScientistQuery request,
        CancellationToken cancellationToken
    )
    {
        bool filter(Scientist s) => s.Id == request.IdOrSlug || s.Slug() == request.IdOrSlug;
        var scientist = await context
            .Scientists.AsAsyncEnumerable()
            .SingleOrDefaultAsync(filter, cancellationToken);

        return scientist?.ToGetScientistResponse();
    }
}
