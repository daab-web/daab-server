using Daab.Modules.Scientists.Persistence;
using Daab.SharedKernel;
using MediatR;

namespace Daab.Modules.Scientists.Features.GetAllScientists;

public class GetAllScientistsQueryHandler(ScientistsContext context)
    : IRequestHandler<GetAllScientistsQuery, PagedResponse<GetAllScientistsResponse>>
{
    public async Task<PagedResponse<GetAllScientistsResponse>> Handle(
        GetAllScientistsQuery request,
        CancellationToken cancellationToken
    )
    {
        var scientists = context.Scientists;
        var response = scientists.ToAllScientistsResponse();

        return await response.ToPagedResponse(request.PaginationOptions);
    }
}
