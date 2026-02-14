using Daab.Modules.Scientists.Persistence;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Scientists.Features.Applications.GetApplication;

public sealed class GetApplicationQueryHandler(ScientistsDbContext context)
    : IRequestHandler<GetApplicationQuery, Fin<ApplicationDto>>
{
    public async Task<Fin<ApplicationDto>> Handle(
        GetApplicationQuery request,
        CancellationToken cancellationToken
    )
    {
        var application = await context.Applications.SingleOrDefaultAsync(
            a => a.Id == request.ApplicationId,
            cancellationToken: cancellationToken
        );

        return application is null
            ? Error.New(StatusCodes.Status404NotFound, "Requested application does not exits")
            : application.ToDto();
    }
}
