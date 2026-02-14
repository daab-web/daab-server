using LanguageExt;
using MediatR;

namespace Daab.Modules.Activities.Features.News.CreateNews;

public sealed class CreateNewsCommand(CreateNewsRequest req) : IRequest<Fin<CreateNewsResponse>>
{
    public string Title { get; } = req.Title;
    public object EditorState { get; } = req.EditorState;
    public string Slug { get; } = req.Slug;

    public string Thumbnail { get; } = req.ThumbnailUri;
    public string? Excerpt { get; } = req.Excerpt;
    public string? AuthorId { get; } = req.AuthorId;
    public string? AuthorName { get; } = req.AuthorName;

    public string? Category { get; } = req.Category;
    public List<string> Tags { get;  } = req.Tags;
}