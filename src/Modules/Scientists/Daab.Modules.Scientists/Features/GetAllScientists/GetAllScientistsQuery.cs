using Daab.Modules.Scientists.Models;
using Daab.Modules.Scientists.Persistence;
using Daab.SharedKernel;
using MediatR;

namespace Daab.Modules.Scientists.Features.GetAllScientists;

public class GetAllScientistsQuery(PageRequest paginationOptions)
    : IRequest<PagedResponse<Scientist>>
{
    public PageRequest PaginationOptions { get; } = paginationOptions;
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

