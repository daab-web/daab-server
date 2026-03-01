using Daab.Modules.Scientists.Models;
using Daab.Modules.Scientists.Persistence;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Scientists.Features.Applications.Approve;

public sealed class ApproveApplicationCommandHandler(ScientistsDbContext context)
    : IRequestHandler<ApproveApplicationCommand, Fin<ApproveApplicationResponse>>
{
    async Task<Fin<ApproveApplicationResponse>> IRequestHandler<
        ApproveApplicationCommand,
        Fin<ApproveApplicationResponse>
    >.Handle(ApproveApplicationCommand request, CancellationToken cancellationToken)
    {
        var application = await context.Applications.SingleOrDefaultAsync(
            a => a.Id == request.ApplicationId,
            cancellationToken: cancellationToken
        );

        if (application is null)
        {
            return Error.New("Requested application does not exist");
        }

        var scientist = new Scientist(
            application.Name,
            application.Surname,
            application.Email,
            application.PhoneNumber,
            application.AdditionalInformation,
            application.AcademicTitle,
            [application.DegreeInstitution],
            application.Residence.Split(','),
            application.FieldOfStudy.Split(',')
        );

        application.Status = ApplicationStatus.Approved;
        var scientistEntity = await context.Scientists.AddAsync(scientist, cancellationToken);
        var statesWritten = await context.SaveChangesAsync(cancellationToken);

        if (statesWritten <= 0)
        {
            return Error.New("Unable to approve application. Please try again later");
        }

        application.Status = ApplicationStatus.Approved;
        await context.SaveChangesAsync(cancellationToken);

        return new ApproveApplicationResponse(scientistEntity.Entity.Id);
    }
}
