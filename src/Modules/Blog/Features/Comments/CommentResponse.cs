using Daab.Modules.Blog.Models;

namespace Daab.Modules.Blog.Features.Comments;

/// <param name="Language">
/// ISO 639-1 code the comment is written in, or <c>null</c> if unknown. Clients compare it with
/// the reader's locale to decide whether to offer a translation.
/// </param>
public sealed record CommentResponse(
    string Id,
    string PostId,
    string AuthorId,
    string AuthorName,
    string Content,
    string? Language,
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
            comment.Content,
            comment.Language,
            comment.CreatedAt
        );
}
