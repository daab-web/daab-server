using Daab.Modules.Activities.Persistence;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Activities.Features.News.AddTranslation;

public sealed class AddTranslationCommandHandler(ActivitiesDbContext ctx)
    : MediatR.IRequestHandler<AddTranslationCommand, Fin<Unit>>
{
    public async Task<Fin<Unit>> Handle(
        AddTranslationCommand request,
        CancellationToken cancellationToken
    )
    {
        var translation = await ctx.NewsTranslations.SingleAsync(
            nt => nt.NewsId == request.NewsId && nt.Locale == request.Locale,
            cancellationToken
        );

        translation.Update(request.Title, request.Excerpt, request.EditorState);

        var statesWritten = await ctx.SaveChangesAsync(cancellationToken);

        return statesWritten <= 0
            ? Error.New(StatusCodes.Status500InternalServerError, "Unable to save translation")
            : Fin.Succ(Prelude.unit);
    }
}
