using System.Threading.Channels;
using Daab.Modules.Activities.Messages;
using Daab.Modules.Activities.Persistence;
using Daab.SharedKernel;
using Daab.SharedKernel.Constants;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Daab.Modules.Activities.Features.News.UpdateNews;

public sealed class UpdateNewsCommandHandler(ActivitiesDbContext context)
    : IRequestHandler<UpdateNewsCommand, Fin<UpdateNewsResponse>>
{
    public async Task<Fin<UpdateNewsResponse>> Handle(
        UpdateNewsCommand request,
        CancellationToken cancellationToken
    )
    {
        var news = await context.News.FindAsync([request.Id], cancellationToken);
        if (news is null)
        {
            return Error.New(
                StatusCodes.Status404NotFound,
                $"News with an Id of {request.Id} not found"
            );
        }

        news.Category = request.Category;
        news.Tags = request.Tags;
        news.PublishedDate = request.PublishedDate;

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateNewsResponse(news.Id);
    }
}
