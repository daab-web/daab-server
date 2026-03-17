using System.Threading.Channels;
using Daab.Modules.Activities.Messages;
using Daab.Modules.Activities.Models;
using Daab.Modules.Activities.Persistence;
using Daab.SharedKernel;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Daab.Modules.Activities.Features.Attachments.AddAttachment;

public sealed record AddAttachmentRequest(string? Caption, IFormFile File);

public class AddAttachmentEndpoint(
    IMediator mediator,
    [FromKeyedServices(ChannelKeys.ThumbnailUpload)] Channel<UploadMessage> channel
) : EndpointWithMapping<AddAttachmentRequest, EmptyResponse, Attachment>
{
    public override void Configure()
    {
        Post("/news/{newsId}/attachments");
        AllowFileUploads();

        // TODO: This should not be public
        AllowAnonymous();
    }

    public override async Task HandleAsync(AddAttachmentRequest req, CancellationToken ct)
    {
        var newsId = Route<string>("newsId");

        if (string.IsNullOrWhiteSpace(newsId))
        {
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        var buffer = new byte[req.File.Length];
        await req.File.OpenReadStream().ReadExactlyAsync(buffer, ct);

        var attachment = await mediator.Send(
            new AddAttachmentCommand
            {
                Caption = req.Caption,
                FileType = req.File.ContentType,
                ParentObjectId = newsId,
            },
            ct
        );

        await channel.Writer.WriteAsync(
            new UploadMessage(newsId, attachment.Id, buffer, MessageType.Attachment),
            ct
        );

        await Send.AcceptedAtAsync<AddAttachmentEndpoint>(
            routeValues: newsId,
            verb: Http.POST,
            generateAbsoluteUrl: true,
            cancellation: ct
        );
    }
}

public sealed class AddAttachmentCommand : IRequest<Attachment>
{
    public string? Caption { get; init; }
    public string? FileType { get; init; }
    public required string ParentObjectId { get; init; }
}

public sealed class AddAttachmentCommandHandler(ActivitiesDbContext ctx)
    : IRequestHandler<AddAttachmentCommand, Attachment>
{
    public async Task<Attachment> Handle(
        AddAttachmentCommand request,
        CancellationToken cancellationToken
    )
    {
        var attachment = new Attachment
        {
            Caption = request.Caption,
            FileType = request.FileType,
            ParentObjectId = request.ParentObjectId,
        };

        var entityEntry = await ctx.Attachments.AddAsync(attachment, cancellationToken);
        await ctx.SaveChangesAsync(cancellationToken);

        return entityEntry.Entity;
    }
}
