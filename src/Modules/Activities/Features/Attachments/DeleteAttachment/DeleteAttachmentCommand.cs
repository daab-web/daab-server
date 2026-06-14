using LanguageExt;
using MediatR;

namespace Daab.Modules.Activities.Features.Attachments.DeleteAttachment;

public class DeleteAttachmentCommand(string articleId, string attachmentId) : IRequest<Fin<bool>>
{
    public string ArticleId { get; } = articleId;
    public string AttachmentId { get; } = attachmentId;
}
