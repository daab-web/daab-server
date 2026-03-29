using System.Text.Json;
using Daab.Modules.Activities.Models;
using Daab.Modules.Activities.Persistence;
using Daab.SharedKernel.Constants;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Daab.Modules.Activities.Features.News.CreateNews;

public sealed class CreateNewsCommandHandler(ActivitiesDbContext context)
    : IRequestHandler<CreateNewsCommand, Fin<CreateNewsResponse>>
{
    public async Task<Fin<CreateNewsResponse>> Handle(
        CreateNewsCommand request,
        CancellationToken cancellationToken
    )
    {
        var news = new Models.News
        {
            Title = request.Title,
            EditorState = JsonSerializer.Serialize(request.EditorState),
            Slug = request.Slug,
            PublishedDate = request.PublishedDate,
            AuthorId = request.AuthorId,
            AuthorName = request.AuthorName,
            Category = request.Category,
            Tags = request.Tags,
            Excerpt = request.Excerpt,
        };

        news.Translations =
        [
            .. Localization.SupportedLocales.Select(locale => new NewsTranslation
            {
                Locale = locale,
                NewsId = news.Id,
            }),
        ];

        var entityEntry = await context.News.AddAsync(news, cancellationToken);
        var statesWritten = await context.SaveChangesAsync(cancellationToken);

        return statesWritten <= 0
            ? Error.New(
                StatusCodes.Status500InternalServerError,
                "Unable to save news... Please try again"
            )
            : new CreateNewsResponse(entityEntry.Entity.Id);
    }
}
