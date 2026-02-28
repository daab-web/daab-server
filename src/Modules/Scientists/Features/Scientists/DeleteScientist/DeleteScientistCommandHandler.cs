using Daab.Modules.Scientists.Persistence;
using FastEndpoints;
using LanguageExt;
using LanguageExt.Common;
using MediatR;

namespace Daab.Modules.Scientists.Features.Scientists.DeleteScientist;

public class DeleteScientistCommandHandler(ScientistsDbContext context)
    : IRequestHandler<DeleteScientistCommand, Fin<bool>>
{
    public async Task<Fin<bool>> Handle(
        DeleteScientistCommand request,
        CancellationToken cancellationToken
    )
    {
        var news = await context.Scientists.FindAsync([request.Id], cancellationToken);
        if (news is null)
        {
            return Error.New($"News with an Id of ${request.Id} not found");
        }
        context.Scientists.Remove(news);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
