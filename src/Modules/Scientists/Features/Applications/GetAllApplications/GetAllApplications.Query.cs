using Daab.SharedKernel;
using LanguageExt;
using MediatR;

namespace Daab.Modules.Scientists.Features.Applications.GetAllApplications;

public sealed record GetAllApplicationsQuery : IRequest<Fin<PagedResponse<ApplicationDto>>>
{
    public int Page { get; init; }
    public int PageSize { get; init; }

    public GetAllApplicationsQuery(GetAllApplicationsRequest request)
    {
        Page = request.Page;
        PageSize = request.PageSize;
    }
}
