using Daab.Modules.Scientists.Persistence;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Scientists.Features.Scientists.UpdateScientist;

public class UpdateScientistCommandHandler(ScientistsDbContext context)
    : IRequestHandler<UpdateScientistCommand, Fin<UpdateScientistResponse>>
{
    public async Task<Fin<UpdateScientistResponse>> Handle(
        UpdateScientistCommand request,
        CancellationToken cancellationToken
    )
    {
        var rowsAffected = await context
            .Scientists.Where(s => s.Id == request.Id)
            .ExecuteUpdateAsync(
                x =>
                    x.SetProperty(s => s.Email, request.Email)
                        .SetProperty(s => s.PhoneNumber, request.PhoneNumber)
                        .SetProperty(s => s.AcademicTitle, request.AcademicTitle)
                        .SetProperty(s => s.Countries, request.Countries)
                        .SetProperty(s => s.Areas, request.Areas)
                        .SetProperty(s => s.Institutions, request.Institution)
                        .SetProperty(s => s.Orcid, request.Orcid)
                        .SetProperty(s => s.LinkedInUrl, request.LinkedInUrl)
                        .SetProperty(s => s.Website, request.Website),
                cancellationToken: cancellationToken
            );

        if (rowsAffected == 0)
        {
            return Error.New(
                StatusCodes.Status404NotFound,
                $"Scientist with an Id of {request.Id} not found."
            );
        }

        await context.SaveChangesAsync(cancellationToken);
        return new UpdateScientistResponse(request.Id);
    }
}
