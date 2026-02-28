using Daab.Modules.Scientists.Persistence;
using LanguageExt;
using LanguageExt.Common;
using MediatR;

namespace Daab.Modules.Scientists.Features.Scientists.UpdateScientist;

public class UpdateScientistCommandHandler(ScientistsDbContext context)
    : IRequestHandler<UpdateScientistCommand, Fin<UpdateScientistResponse>>
{
    public async Task<Fin<UpdateScientistResponse>> Handle(
        UpdateScientistCommand request,
        CancellationToken cancellationToken
    )
    {
        var scientist = await context.Scientists.FindAsync([request.Id], cancellationToken);
        if (scientist is null)
        {
            return Error.New($"Scientist with an Id of {request.Id} not found.");
        }

        scientist.Email = request.Email;
        scientist.PhoneNumber = request.PhoneNumber;
        scientist.AcademicTitle = request.AcademicTitle;
        scientist.LastName = request.LastName;
        scientist.Countries = request.Countries;
        scientist.Areas = request.Areas;
        scientist.Institution = request.Institution;
        scientist.FirstName = request.FirstName;
        scientist.Description = request.Description;

        context.Scientists.Update(scientist);
        await context.SaveChangesAsync(cancellationToken);
        return new UpdateScientistResponse(scientist.Id);
    }
}
