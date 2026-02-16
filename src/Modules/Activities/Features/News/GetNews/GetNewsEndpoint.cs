using Daab.SharedKernel.Extensions;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using ZiggyCreatures.Caching.Fusion;

namespace Daab.Modules.Activities.Features.News.GetNews;

public class GetNewsEndpoint(IMediator mediator) : EndpointWithoutRequest<GetNewsResponse>
{
    public override void Configure()
    {
        Get("/news/{idOrSlug}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var idOrSlug = Route<string>("idOrSlug");

        if (string.IsNullOrWhiteSpace(idOrSlug))
        {
            await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
            return;
        }

        // var cached = await cache.TryGetAsync<GetNewsResponse>(HttpContext.Request.GetEncodedUrl(), token: ct);
        // if (cached.HasValue)
        // {
        //     return;
        // }

        var res = await mediator.Send(new GetNewsQuery(idOrSlug), ct);

        await res.Match(
            news => Send.OkAsync(news, ct),
            err => Send.ResultAsync(TypedResults.Problem(err.ToProblemDetails(HttpContext)))
        );
    }
}
