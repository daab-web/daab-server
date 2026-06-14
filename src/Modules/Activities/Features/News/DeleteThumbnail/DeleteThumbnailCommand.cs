using LanguageExt;
using MediatR;

namespace Daab.Modules.Activities.Features.News.DeleteThumbnail;

public class DeleteThumbnailCommand(string newsId) : IRequest<Fin<bool>>
{
    public string NewsId { get; } = newsId;
}
