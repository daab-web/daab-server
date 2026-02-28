using Daab.Modules.Activities.Persistence;
using LanguageExt;
using LanguageExt.Common;
using MediatR;

namespace Daab.Modules.Activities.Features.News.DeleteNews;

public sealed class DeleteNewsCommandHandler(ActivitiesDbContext context)
    : IRequestHandler<DeleteNewsCommand, Fin<bool>>
{
    public async Task<Fin<bool>> Handle(
        DeleteNewsCommand request,
        CancellationToken cancellationToken
    )
    {
        var news = await context.News.FindAsync([request.Id], cancellationToken);
        if (news is null)
        {
            return Error.New($"News with an Id of ${request.Id} not found");
        }
        context.News.Remove(news);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
