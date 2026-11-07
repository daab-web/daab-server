using Daab.Modules.Blog.Models;

namespace Daab.Modules.Blog.Features.Comments;

/// <param name="SourceLocale">The language the commenter wrote in.</param>
/// <param name="Texts">The comment in every supported language, keyed by locale.</param>
public sealed record CommentResponse(
    string Id,
    string PostId,
    string AuthorId,
    string AuthorName,
    string SourceLocale,
    IReadOnlyDictionary<string, string> Texts,
    DateTime CreatedAt
);

public static class CommentMappings
{
    public static CommentResponse ToResponse(this Comment comment) =>
        new(
            comment.Id,
            comment.PostId,
            comment.AuthorId,
            comment.AuthorName,
            comment.SourceLocale,
            comment.Texts.ToDictionary(t => t.Locale, t => t.Content, StringComparer.Ordinal),
            comment.CreatedAt
        );
}
