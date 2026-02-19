using Daab.Modules.Scientists.Persistence;
using Daab.SharedKernel;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Scientists.Features.Scientists.GetAllScientists;

public class GetAllScientistsQueryHandler(ScientistsDbContext context)
    : IRequestHandler<GetAllScientistsQuery, PagedResponse<GetAllScientistsResponse>>
{
    public async Task<PagedResponse<GetAllScientistsResponse>> Handle(
        GetAllScientistsQuery request,
        CancellationToken cancellationToken
    )
    {
        var scientists = context.Scientists.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Country))
        {
            scientists = scientists.Where(s => s.Countries.Contains(request.Country));
        }

        if (!string.IsNullOrWhiteSpace(request.Area))
        {
            scientists = scientists.Where(s => s.Areas.Contains(request.Area));
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            scientists = scientists.Where(s =>
                s.FirstName.Contains(request.Search)
                || s.LastName.Contains(request.Search)
                || s.Institution.Contains(request.Search)
                || s.Areas.Contains(request.Search)
            );
        }

        var response = scientists.ToAllScientistsResponse();
        return await response.ToPagedResponseAsync(
            new PageRequest { PageNumber = request.PageNumber, PageSize = request.PageSize },
            cancellationToken: cancellationToken
        );
    }
}
