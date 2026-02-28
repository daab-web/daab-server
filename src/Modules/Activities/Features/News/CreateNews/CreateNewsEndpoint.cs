using Daab.SharedKernel.Extensions;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Daab.Modules.Activities.Features.News.CreateNews;

public class CreateNewsEndpoint(IMediator mediator)
    : Endpoint<CreateNewsRequest, CreateNewsResponse>
{
    public override void Configure()
    {
        Post("/news");

        AllowFormData();

        // TODO: This should not be public
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateNewsRequest req, CancellationToken ct)
    {
        if (req.Thumbnail?.ContentType.StartsWith("image/") == false)
        {
            await Send.ResultAsync(
                TypedResults.BadRequest("Invalid thumbnail format. Only image files are allowed.")
            );
            return;
        }

        var res = await mediator.Send(new CreateNewsCommand(req), ct);

        await res.Match(
            entity =>
                Send.CreatedAtAsync(
                    endpointName: nameof(CreateNewsEndpoint),
                    routeValues: null,
                    responseBody: entity,
                    generateAbsoluteUrl: true,
                    cancellation: ct
                ),
            err => err.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
        );
    }
}
