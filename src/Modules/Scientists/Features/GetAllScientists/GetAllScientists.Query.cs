using Daab.SharedKernel;
using MediatR;

namespace Daab.Modules.Scientists.Features.GetAllScientists;

public class GetAllScientistsQuery
    : IRequest<PagedResponse<GetAllScientistsResponse>>
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

