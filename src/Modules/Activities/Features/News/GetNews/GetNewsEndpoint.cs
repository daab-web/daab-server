using Daab.SharedKernel.Extensions;
using FastEndpoints;
using MediatR;

namespace Daab.Modules.Activities.Features.News.GetNews;

public class GetNewsEndpoint(IMediator mediator) : Endpoint<GetNewsRequest, GetNewsResponse>
{
    public override void Configure()
    {
        Get("/news/{idOrSlug}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetNewsRequest req, CancellationToken ct)
    {
        var res = await mediator.Send(new GetNewsQuery(req.IdOrSlug, req.Locale), ct);

        await res.Match(
            news => Send.OkAsync(news, ct),
            err => err.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
        );
    }
}
