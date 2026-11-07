using Daab.Modules.Blog.Common;
using Daab.SharedKernel.Extensions;
using Daab.SharedKernel.Middlewares;
using FastEndpoints;
using MediatR;

namespace Daab.Modules.Blog.Features.Translations.SuggestTranslation;

/// <summary>Signed-in users only, since every call costs an AI request.</summary>
public sealed class SuggestTranslationEndpoint(IMediator mediator)
    : Endpoint<SuggestTranslationRequest, SuggestTranslationResponse>
{
    public override void Configure()
    {
        Post("/blog/translations/suggest");
        AuthSchemes(BlogUser.Scheme);
        PreProcessor<LocaleCollectionPreProcessor>();
    }

    public override async Task HandleAsync(SuggestTranslationRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(
            new SuggestTranslationCommand(req.SourceLocale, req.TargetLocale, req.Texts),
            ct
        );

        await result.Match(
            suggestion => Send.OkAsync(suggestion, ct),
            err => err.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
        );
    }
}
