using Daab.SharedKernel.Extensions;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Daab.Modules.Activities.Features.News.CreateNews;


// TODO: thumbnail upload
public class CreateNewsEndpoint(IMediator mediator) : Endpoint<CreateNewsRequest, CreateNewsResponse>
{
    public override void Configure()
    {
        Post("/api/news");

        // TODO: This should not be public
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateNewsRequest req, CancellationToken ct)
    {
        var res = await mediator.Send(new CreateNewsCommand(req), ct);

        await res.Match(
            entity => Send.CreatedAtAsync(
                endpointName: nameof(CreateNewsEndpoint),
                routeValues: null,
                responseBody: entity,
                generateAbsoluteUrl: true,
                cancellation: ct
            ),
            err => Send.ResultAsync(TypedResults.Problem(err.ToProblemDetails(HttpContext))));
    }
}