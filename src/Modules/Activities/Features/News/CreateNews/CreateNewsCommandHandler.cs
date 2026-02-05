using System.Text.Json;
using Daab.Modules.Activities.Persistence;
using LanguageExt;
using LanguageExt.Common;
using MediatR;

namespace Daab.Modules.Activities.Features.News.CreateNews;

public sealed class CreateNewsCommandHandler(ActivitiesDbContext context)
    : IRequestHandler<CreateNewsCommand, Fin<CreateNewsResponse>>
{
    public async Task<Fin<CreateNewsResponse>> Handle(CreateNewsCommand request, CancellationToken cancellationToken)
    {
        var news = new Models.News
        {
            Title = request.Title,
            EditorState = JsonSerializer.Serialize(request.EditorState),
            Thumbnail = ""
        };

        var entityEntry = await context.News.AddAsync(news, cancellationToken);
        var statesWritten = await context.SaveChangesAsync(cancellationToken);

        return statesWritten <= 0
            ? Error.New("Unable to save news... Please try again")
            : new CreateNewsResponse(entityEntry.Entity.Id);
    }
}