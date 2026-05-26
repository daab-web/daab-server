using Daab.Modules.Scientists.Models;
using Daab.Modules.Scientists.Persistence;
using Daab.SharedKernel.Entities;
using Daab.SharedKernel.Extensions;
using Daab.SharedKernel.Middlewares;
using Daab.SharedKernel.Options;
using FastEndpoints;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Unit = LanguageExt.Unit;

namespace Daab.Modules.Scientists.Features.Scientists.AddTranslation;

public sealed record AddTranslationRequest(
    string Locale,
    string ScientistId,
    string FirstName,
    string LastName,
    string Description
) : ILocalized;

public sealed class AddTranslationCommand(AddTranslationRequest req) : IRequest<Fin<Unit>>
{
    public string Locale { get; } = req.Locale;
    public string ScientistId { get; } = req.ScientistId;
    public string FirstName { get; } = req.FirstName;
    public string LastName { get; } = req.LastName;
    public string Description { get; } = req.Description;
}

public sealed class AddTranslationCommandHandler(
    ScientistsDbContext ctx,
    IOptionsMonitor<LocaleOptions> opts
) : IRequestHandler<AddTranslationCommand, Fin<Unit>>
{
    private readonly LocaleOptions _localeOptions = opts.CurrentValue;

    public async Task<Fin<Unit>> Handle(
        AddTranslationCommand request,
        CancellationToken cancellationToken
    )
    {
        var scientist = await ctx
            .Scientists.Include(s => s.Translations)
            .SingleOrDefaultAsync(
                st => st.Id == request.ScientistId,
                cancellationToken: cancellationToken
            );

        if (scientist is null)
        {
            return Fin.Fail<Unit>(Error.New("Scientist not found"));
        }

        scientist.AddOrUpdateTranslation(
            request.Locale,
            request.FirstName,
            request.LastName,
            request.Description
        );

        var supportedLocales = _localeOptions.SupportedLocales;
        var presentLocales = scientist.Translations.Select(t => t.Locale);

        if (supportedLocales.Order().SequenceEqual(presentLocales.Order()))
        {
            scientist.Status = EntityStatus.ReadyToPublish;
        }

        await ctx.SaveChangesAsync(cancellationToken);

        return Prelude.unit;
    }
}

public class AddTranslationEndpoint(IMediator mediator) : Endpoint<AddTranslationRequest>
{
    public override void Configure()
    {
        Post("/scientists/{scientistId}/translations");
        AllowAnonymous();
    }

    public override async Task HandleAsync(AddTranslationRequest req, CancellationToken ct)
    {
        var res = await mediator.Send(new AddTranslationCommand(req), cancellationToken: ct);

        await res.Match(
            _ => Send.NoContentAsync(ct),
            err => err.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
        );
    }
}
