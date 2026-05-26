using Daab.SharedKernel;
using MediatR;

namespace Daab.Modules.Scientists.Features.Scientists.GetAllScientists;

public class GetAllScientistsQuery(GetAllScientistsRequest request, string locale)
    : IRequest<PagedResponse<GetAllScientistsResponse>>
{
    public string Locale { get; } = locale;
    public string? Search { get; } = request.Search;
    public string? Country { get; } = request.Country;
    public string? Area { get; } = request.Area;
    public int PageNumber { get; } = request.Page;
    public int PageSize { get; } = request.PageSize;
}
