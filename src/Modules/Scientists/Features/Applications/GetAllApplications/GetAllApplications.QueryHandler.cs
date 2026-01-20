using Daab.Modules.Scientists.Features.Applications;
using Daab.Modules.Scientists.Persistence;
using Daab.SharedKernel;
using LanguageExt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Scientists.Features.GetAllApplications;

public sealed class GetAllApplicationsQueryHandler(ScientistsContext context)
    : IRequestHandler<GetAllApplicationsQuery, Fin<PagedResponse<ApplicationDto>>>
{
    public async Task<Fin<PagedResponse<ApplicationDto>>> Handle(
        GetAllApplicationsQuery request,
        CancellationToken cancellationToken
    )
    {
        var applications = await context
            .Applications.AsNoTracking()
            .ToDto()
            .ToPagedResponseAsync(
                new PageRequest { PageNumber = request.Page, PageSize = request.PageSize },
                cancellationToken
            );

        return applications;
    }
}
