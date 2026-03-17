using Daab.SharedKernel;
using LanguageExt;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Daab.Modules.Activities.Features.News.CreateNews;

public sealed class CreateNewsCommand(CreateNewsRequest req) : IRequest<Fin<CreateNewsResponse>>
{
    public string Title { get; } = req.Title;
    public object EditorState { get; } = req.EditorState;
    public string Slug { get; } = SlugHelper.GenerateSlug(req.Title);

    public IFormFile? Thumbnail { get; } = req.Thumbnail;
    public string? Excerpt { get; } = req.Excerpt;
    public string? AuthorId { get; } = req.AuthorId;
    public string? AuthorName { get; } = req.Author;

    public string? Category { get; } = req.Category;
    public List<string> Tags { get; } = req.Tags;
    public DateTime PublishedDate { get; set; } = req.PublishedDate;
}
