using Daab.Modules.Scientists.Models;
using Daab.Modules.Scientists.Persistence;
using Daab.SharedKernel;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Scientists.Features.Directors.GetAllDirectors;

public class GetAllDirectorsQueryHandler(ScientistsDbContext context)
    : IRequestHandler<GetAllDirectorsQuery, IEnumerable<Director>>
{
    public async Task<IEnumerable<Director>> Handle(
        GetAllDirectorsQuery request,
        CancellationToken cancellationToken
    )
    {
        var directors = await context
            .Directors.AsNoTracking()
            .Include(d => d.Scientist)
            .ToListAsync(cancellationToken);

        return directors;
    }
}
