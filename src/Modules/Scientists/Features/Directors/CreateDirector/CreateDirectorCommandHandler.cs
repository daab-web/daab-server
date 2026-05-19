using Daab.Modules.Scientists.Models;
using Daab.Modules.Scientists.Persistence;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Scientists.Features.Directors.CreateDirector;

public class CreateDirectorCommandHandler(ScientistsDbContext context)
    : IRequestHandler<CreateDirectorCommand, Fin<CreateDirectorResponse>>
{
    public async Task<Fin<CreateDirectorResponse>> Handle(
        CreateDirectorCommand request,
        CancellationToken cancellationToken
    )
    {
        if (!await context.Scientists.AnyAsync(s => s.Id == request.ScientistId, cancellationToken))
        {
            return Error.New(
                StatusCodes.Status404NotFound,
                $"Scientist with an Id of {request.ScientistId} not found"
            );
        }

        var director = new Director { ScientistId = request.ScientistId };
        foreach ((string locale, string translation) in request.Translations)
        {
            director.RoleTranslations.Add(locale, translation);
        }

        await context.Directors.AddAsync(director, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return new CreateDirectorResponse(director.Id);
    }
}
