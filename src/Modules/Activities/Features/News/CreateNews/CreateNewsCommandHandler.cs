using Daab.Modules.Activities.Persistence;
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
            Slug = request.Slug,
            PublishedDate = request.PublishedDate,
            AuthorId = request.AuthorId,
            AuthorName = request.AuthorName,
            Category = request.Category,
            Tags = request.Tags,
        };

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
