using Daab.SharedKernel.Extensions;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Daab.Modules.Activities.Features.News.GetNews;

public class GetNewsEndpoint(IMediator mediator) : EndpointWithoutRequest<GetNewsResponse>
{
    public override void Configure()
    {
        Get("/news/{newsId}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var newsId = Route<string>("newsId");

        if (string.IsNullOrWhiteSpace(newsId))
        {
            await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
            return;
        }

        var res = await mediator.Send(new GetNewsQuery(newsId), ct);

        await res.Match(
            news => Send.OkAsync(news, ct),
            err => Send.ResultAsync(TypedResults.Problem(err.ToProblemDetails(HttpContext)))
        );
    }
}