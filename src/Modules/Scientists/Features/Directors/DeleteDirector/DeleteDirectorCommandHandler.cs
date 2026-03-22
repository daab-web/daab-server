using Daab.Modules.Scientists.Persistence;
using LanguageExt;
using LanguageExt.Common;
using MediatR;

namespace Daab.Modules.Scientists.Features.Directors.DeleteDirector;

public class DeleteDirectorCommandHandler(ScientistsDbContext context)
    : IRequestHandler<DeleteDirectorCommand, Fin<bool>>
{
    public async Task<Fin<bool>> Handle(
        DeleteDirectorCommand request,
        CancellationToken cancellationToken
    )
    {
        var director = await context.Directors.FindAsync([request.Id], cancellationToken);
        if (director is null)
        {
            return Error.New(404, $"Director with an Id of {request.Id} not found");
        }

        context.Directors.Remove(director);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
