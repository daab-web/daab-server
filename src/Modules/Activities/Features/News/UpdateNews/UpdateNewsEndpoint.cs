using Daab.SharedKernel.Extensions;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Daab.Modules.Activities.Features.News.UpdateNews;

public class UpdateNewsEndpoint(IMediator mediator)
    : Endpoint<UpdateNewsRequest, UpdateNewsResponse>
{
    public override void Configure()
    {
        Put("/news/{id}");
        AllowFileUploads();

        // TODO: This should not be public
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateNewsRequest req, CancellationToken ct)
    {
        var id = Route<string>("id");
        if (string.IsNullOrEmpty(id))
        {
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        if (req.Thumbnail?.ContentType.StartsWith("image/") is false)
        {
            await Send.ResultAsync(
                TypedResults.BadRequest("Invalid thumbnail format. Only image files are allowed.")
            );
            return;
        }

        var res = await mediator.Send(new UpdateNewsCommand(id, req), ct);

        await res.Match(
            entity => Send.OkAsync(entity, cancellation: ct),
            err => err.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
        );
    }
}
