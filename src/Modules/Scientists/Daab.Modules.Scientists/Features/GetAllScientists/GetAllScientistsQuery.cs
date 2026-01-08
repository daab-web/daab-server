using Daab.Modules.Scientists.Models;
using Daab.Modules.Scientists.Persistence;
using Daab.SharedKernel;
using MediatR;

namespace Daab.Modules.Scientists.Features.GetAllScientists;

public class GetAllScientistsQuery
    : IRequest<PagedResponse<Scientist>>
{
    public PageRequest PaginationOptions { get; }

    public GetAllScientistsQuery(GetAllScientistsRequest request)
    {
        PaginationOptions = new PageRequest
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
        };
    }
}

public class GetAllScientistsQueryHandler(ScientistsContext context)
    : IRequestHandler<GetAllScientistsQuery, PagedResponse<Scientist>>
{
    public async Task<PagedResponse<Scientist>> Handle(
        GetAllScientistsQuery request,
        CancellationToken cancellationToken
    )
    {
        var scientists = context.Scientists.AsQueryable();

        return await scientists.ToPagedResponseAsync(request.PaginationOptions, cancellationToken);
    }
}

