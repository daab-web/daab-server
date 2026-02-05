using System.Text.Json;
using Daab.Modules.Activities.Persistence;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Daab.Modules.Activities.Features.News.GetNews;

public sealed class GetNewsQueryHandler(ActivitiesDbContext context)
    : IRequestHandler<GetNewsQuery, Fin<GetNewsResponse>>
{
    public async Task<Fin<GetNewsResponse>> Handle(GetNewsQuery request, CancellationToken cancellationToken)
    {
        var news = await context.News.FindAsync([request.Id], cancellationToken: cancellationToken);
        if (news is null)
        {
            return Error.New(StatusCodes.Status404NotFound, "Requested news does not exist");
        }

        var state = JsonSerializer.Deserialize<object>(news.EditorState);

        return new GetNewsResponse(news.Id, news.Title, news.Thumbnail, state);
    }
}