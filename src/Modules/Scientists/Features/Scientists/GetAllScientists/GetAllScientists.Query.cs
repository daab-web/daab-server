using Daab.SharedKernel;
using MediatR;

namespace Daab.Modules.Scientists.Features.Scientists.GetAllScientists;

public class GetAllScientistsQuery : IRequest<PagedResponse<GetAllScientistsResponse>>
{
    public string? Country { get; }
    public int PageNumber { get; }
    public int PageSize { get; }

    public GetAllScientistsQuery(GetAllScientistsRequest request)
    {
        PageNumber = request.Page;
        PageSize = request.PageSize;
        Country = request.Country;
    }
}
