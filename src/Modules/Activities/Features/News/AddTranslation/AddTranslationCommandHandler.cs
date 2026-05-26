using Daab.Modules.Activities.Persistence;
using Daab.SharedKernel.Entities;
using Daab.SharedKernel.Options;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Daab.Modules.Activities.Features.News.AddTranslation;

public sealed class AddTranslationCommandHandler(
    ActivitiesDbContext ctx,
    IOptionsMonitor<LocaleOptions> opts
) : MediatR.IRequestHandler<AddTranslationCommand, Fin<Unit>>
{
    private readonly LocaleOptions _localeOptions = opts.CurrentValue;

    public async Task<Fin<Unit>> Handle(
        AddTranslationCommand request,
        CancellationToken cancellationToken
    )
    {
        var news = await ctx
            .News.Include(n => n.Translations)
            .SingleOrDefaultAsync(n => n.Id == request.NewsId, cancellationToken);

        if (news is null)
        {
            return Fin.Fail<Unit>(Error.New("News not found"));
        }

        news.AddOrUpdateTranslation(
            request.Locale,
            request.Title,
            request.Excerpt,
            request.EditorState
        );

        var supportedLocales = _localeOptions.SupportedLocales;
        var presentLocales = news.Translations.Select(t => t.Locale);

        if (supportedLocales.Order().SequenceEqual(presentLocales.Order()))
        {
            news.Status = EntityStatus.ReadyToPublish;
        }

        await ctx.SaveChangesAsync(cancellationToken);

        return Fin.Succ(Prelude.unit);
    }
}
