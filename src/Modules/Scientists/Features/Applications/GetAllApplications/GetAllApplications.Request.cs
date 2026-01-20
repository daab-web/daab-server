using FastEndpoints;

namespace Daab.Modules.Scientists.Features.GetAllApplications;

public sealed record GetAllApplicationsRequest
{
    [QueryParam]
    public int Page { get; init; } = 1;

    [QueryParam]
    public int PageSize { get; init; } = 10;
}
