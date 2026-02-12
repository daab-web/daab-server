using Daab.Modules.Scientists.Features.GetAllScientists;
using Daab.Modules.Scientists.Persistence;
using Daab.SharedKernel;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZiggyCreatures.Caching.Fusion;

namespace Daab.Modules.Scientists.Features.Scientists.GetAllScientists;

public class GetAllScientistsQueryHandler(ScientistsDbContext context, IFusionCache cache)
    : IRequestHandler<GetAllScientistsQuery, PagedResponse<GetAllScientistsResponse>>
{
    public async Task<PagedResponse<GetAllScientistsResponse>> Handle(
        GetAllScientistsQuery request,
        CancellationToken cancellationToken
    )
    {
        return await cache.GetOrSetAsync(
            nameof(GetAllScientistsEndpoint),
            async _ =>
            {
                var scientists = context.Scientists.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(request.Country))
                {
                    scientists = scientists.Where(s => s.Countries.Contains(request.Country));
                }

                var response = scientists.ToAllScientistsResponse();
                return await response.ToPagedResponseAsync(
                    new PageRequest
                    {
                        PageNumber = request.PageNumber,
                        PageSize = request.PageSize,
                    },
                    cancellationToken: cancellationToken
                );
            },
            token: cancellationToken
        );
    }
}
