using Daab.Modules.Activities.Models;
using Daab.Modules.Activities.Persistence;
using Daab.SharedKernel.Entities;
using FastEndpoints;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Activities.Features.News.GetTranslations;

public sealed record GetTranslationsRequest
{
    public string? Include { get; init; }
}

public class GetTranslationsEndpoint(IMediator mediator)
    : EndpointWithMapping<GetTranslationsRequest, GetTranslationsResponse, NewsTranslation[]>
{
    public override void Configure()
    {
        Get("/news/translations");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetTranslationsRequest request, CancellationToken ct)
    {
        var response = await mediator.Send(
            new GetTranslationsQuery(request.Include),
            cancellationToken: ct
        );

        await Send.OkAsync(MapFromEntity(response), ct);
    }

    public override GetTranslationsResponse MapFromEntity(NewsTranslation[] e)
    {
        return new GetTranslationsResponse
        {
            Translations = e.Select(t => new GetTranslationsResponse.TranslationDto(
                    t.NewsId,
                    t.Locale,
                    t.Status.ToString(),
                    t.Title,
                    t.Excerpt,
                    t.EditorState
                ))
                .ToArray(),
        };
    }
}

public sealed class GetTranslationsResponse
{
    public TranslationDto[] Translations { get; init; } = [];

    public sealed record TranslationDto(
        string NewsId,
        string Locale,
        string Status,
        string? Title,
        string? Excerpt,
        string? EditorState
    );
}

public sealed class GetTranslationsQuery(string? filter) : IRequest<NewsTranslation[]>
{
    public string? Filter { get; } = filter;
}

public sealed class GetTranslationsQueryHandler(ActivitiesDbContext ctx)
    : IRequestHandler<GetTranslationsQuery, NewsTranslation[]>
{
    public Task<NewsTranslation[]> Handle(
        GetTranslationsQuery request,
        CancellationToken cancellationToken
    )
    {
        var hasFilter = Enum.TryParse<TranslationStatus>(
            request.Filter,
            ignoreCase: true,
            out var filter
        );

        var translations = ctx.NewsTranslations.AsNoTracking();

        if (hasFilter)
        {
            translations = translations.Where(t => t.Status == filter);
        }

        return translations.ToArrayAsync(cancellationToken: cancellationToken);
    }
}
