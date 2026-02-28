using Daab.Modules.Activities.Persistence;
using Daab.SharedKernel;
using LanguageExt;
using LanguageExt.Common;
using MediatR;

namespace Daab.Modules.Activities.Features.News.DeleteNews;

public sealed class DeleteNewsCommandHandler(ActivitiesDbContext context, IBlobStorage blobStorage)
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

        if (
            news.Thumbnail is not null
            && await blobStorage.ExistsAsync(
                "activities",
                $"news/{news.Id}.webp",
                cancellationToken
            )
        )
        {
            await blobStorage.DeleteAsync("activities", $"news/{news.Id}.webp", cancellationToken);
        }

        context.News.Remove(news);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
