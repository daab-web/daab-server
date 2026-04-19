using Daab.Modules.Scientists.Models;
using Daab.Modules.Scientists.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Scientists.Features.Scientists.GetScientist;

public class GetScientistQueryHandler(ScientistsDbContext context)
    : IRequestHandler<GetScientistQuery, GetScientistResponse?>
{
    public async Task<GetScientistResponse?> Handle(
        GetScientistQuery request,
        CancellationToken cancellationToken
    )
    {
        var scientist = await context
            .Scientists.AsNoTracking()
            .Include(s => s.Publications)
            .Include(s => s.Translations.Where(t => t.Locale == request.Locale))
            .AsAsyncEnumerable()
            .SingleOrDefaultAsync(Filter, cancellationToken);

        return scientist?.ToGetScientistResponse();
        bool Filter(Scientist s) => s.Id == request.IdOrSlug || s.Slug == request.IdOrSlug;
    }
}
