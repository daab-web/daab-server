using System.Threading.Channels;
using Daab.Modules.Activities.Messages;
using Daab.Modules.Activities.Persistence;
using Daab.SharedKernel.Constants;
using Daab.SharedKernel.Extensions;
using FastEndpoints;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Daab.Modules.Activities.Features.News.SetThumbnail;

public sealed record SetThumbnailRequest(string NewsId, IFormFile Image);

public sealed record SetThumbnailResponse(string NewsId);

public class SetThumbnailEndpoint(IMediator mediator)
    : Endpoint<SetThumbnailRequest, SetThumbnailResponse>
{
    public override void Configure()
    {
        Post("/news/{newsId}/thumbnail");
        AllowFileUploads();

        AllowAnonymous();
    }

    public override async Task HandleAsync(SetThumbnailRequest req, CancellationToken ct)
    {
        if (!req.Image.ContentType.StartsWith("image/"))
        {
            await Send.ErrorsAsync(400, ct);
            return;
        }

        var res = await mediator.Send(
            new SetThumbnailCommand { Image = req.Image, NewsId = req.NewsId },
            ct
        );

        await res.Match(
            Send.OkAsync,
            err => err.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
        );
    }
}

public sealed class SetThumbnailCommand : IRequest<Fin<SetThumbnailResponse>>
{
    public required IFormFile Image { get; init; }
    public required string NewsId { get; init; }
}

public sealed class SetThumbnailCommandHandler(
    ActivitiesDbContext ctx,
    [FromKeyedServices(ChannelKeys.ThumbnailUpload)] Channel<UploadMessage> channel
) : IRequestHandler<SetThumbnailCommand, Fin<SetThumbnailResponse>>
{
    public async Task<Fin<SetThumbnailResponse>> Handle(
        SetThumbnailCommand request,
        CancellationToken cancellationToken
    )
    {
        var news = await ctx.News.FindAsync([request.NewsId], cancellationToken);

        if (news is null)
        {
            return Error.New(
                StatusCodes.Status404NotFound,
                "Requested news does not exist or is not yet created"
            );
        }

        var buff = new byte[request.Image.Length];
        await using var readStream = request.Image.OpenReadStream();
        await readStream.ReadExactlyAsync(buff, cancellationToken);

        await channel.Writer.WriteAsync(
            new UploadMessage(news.Id, news.Id, buff, MessageType.Thumbnail),
            cancellationToken
        );

        return new SetThumbnailResponse(news.Id);
    }
}
