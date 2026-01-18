using FastEndpoints;

namespace Daab.Modules.Scientists.Features.GetAllScientists;

public sealed record GetAllScientistsRequest
{
    [QueryParam]
    public int PageNumber { get; set; }

    [QueryParam]
    public int PageSize { get; set; }

    [QueryParam]
    public string? Country { get; init; }
}
