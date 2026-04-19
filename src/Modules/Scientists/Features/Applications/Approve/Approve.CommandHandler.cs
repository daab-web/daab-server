using Daab.Modules.Scientists.Models;
using Daab.Modules.Scientists.Persistence;
using Daab.SharedKernel;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
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
            return Error.New(StatusCodes.Status404NotFound, "Requested application does not exist");
        }

        var slug = SlugHelper.GenerateSlug($"{application.Name} {application.Surname}");
        var slugExists = await context.Scientists.AnyAsync(x => x.Slug == slug, cancellationToken);

        var scientist = new Scientist(
            application.Email,
            application.PhoneNumber,
            application.AcademicTitle,
            [application.DegreeInstitution],
            application.Residence.Split(','),
            application.FieldOfStudy.Split(','),
            null,
            null,
            null,
            null,
            null
        )
        {
            Slug = string.Empty,
        };

        scientist.Slug = slugExists ? $"{slug}-{scientist.Id[..5]}" : slug;

        application.Status = ApplicationStatus.Approved;
        var scientistEntity = await context.Scientists.AddAsync(scientist, cancellationToken);
        var statesWritten = await context.SaveChangesAsync(cancellationToken);

        if (statesWritten <= 0)
        {
            return Error.New(
                StatusCodes.Status500InternalServerError,
                "Unable to approve application. Please try again later"
            );
        }

        await context.SaveChangesAsync(cancellationToken);

        return new ApproveApplicationResponse(scientistEntity.Entity.Id);
    }
}
