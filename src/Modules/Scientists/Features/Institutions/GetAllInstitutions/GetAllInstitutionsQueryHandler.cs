using Daab.Modules.Scientists.Persistence;
using LanguageExt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Scientists.Features.Institutions.GetAllInstitutions;

public class GetAllInstitutionsQueryHandler(ScientistsDbContext context)
    : IRequestHandler<GetAllInstitutionsQuery, GetAllInstitutionsResponse>
{
    public Task<GetAllInstitutionsResponse> Handle(
        GetAllInstitutionsQuery request,
        CancellationToken cancellationToken
    )
    {
        var institutions = context
            .Scientists.AsNoTracking()
            .Select(s => s.Institutions)
            .Flatten()
            .Order()
            .Distinct()
            .ToArray();

        return Task.FromResult(new GetAllInstitutionsResponse(institutions));
    }
}
