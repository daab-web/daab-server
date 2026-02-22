using System.Diagnostics.CodeAnalysis;
using LanguageExt;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Daab.Modules.Activities.Features.News.UpdateNews;

public sealed class UpdateNewsCommand : IRequest<Fin<UpdateNewsResponse>>
{
    [SetsRequiredMembers]
    public UpdateNewsCommand(string id, UpdateNewsRequest request)
    {
        Id = id;
        Title = request.Title;
        Thumbnail = request.Thumbnail;
        Excerpt = request.Excerpt;
        Category = request.Category;
        EditorState = request.EditorState;
        Tags = request.Tags;
    }

    public required string Id { get; init; }
    public string Title { get; init; }
    public IFormFile? Thumbnail { get; init; }
    public string? Excerpt { get; init; }
    public string? Category { get; init; }
    public string EditorState { get; init; }
    public List<string> Tags { get; init; }
}
