using Daab.Modules.Activities.Models;
using Daab.Modules.Activities.Persistence;
using Daab.SharedKernel.Extensions;
using FastEndpoints;
using LanguageExt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Activities.Features.Attachments.LoadAttachments;

public sealed record AttachmentDto(string Id, string FileUrl, string? Caption, string? FileType);

public sealed record LoadAttachmentsResponse(string NewsId, List<AttachmentDto> Attachments);

public class LoadAttachmentsEndpoint(IMediator mediator)
    : EndpointWithoutRequest<LoadAttachmentsResponse>
{
    public override void Configure()
    {
        Get("/news/{newsId}/attachments");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var newsId = Route<string>("newsId");

        if (string.IsNullOrWhiteSpace(newsId))
        {
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        var res = await mediator.Send(
            new LoadAttachmentsQuery { NewsId = newsId },
            cancellationToken: ct
        );

        await res.Match(
            attachments =>
            {
                var dtos = attachments.ConvertAll(a => new AttachmentDto(
                    a.Id,
                    a.FileUrl,
                    a.Caption,
                    a.FileType
                ));
                return Send.OkAsync(new LoadAttachmentsResponse(newsId, dtos));
            },
            err => err.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
        );
    }
}

public sealed class LoadAttachmentsQuery : IRequest<Fin<List<Attachment>>>
{
    public required string NewsId { get; set; }
}

public sealed class LoadAttachmentsQueryHandler(ActivitiesDbContext ctx)
    : IRequestHandler<LoadAttachmentsQuery, Fin<List<Attachment>>>
{
    public async Task<Fin<List<Attachment>>> Handle(
        LoadAttachmentsQuery request,
        CancellationToken cancellationToken
    )
    {
        return await ctx
            .Attachments.Where(a => a.ParentObjectId == request.NewsId)
            .ToListAsync(cancellationToken);
    }
}
