using Daab.Modules.Blog.Models;

namespace Daab.Modules.Blog.Features.Posts;

/// <summary>A published post as listed on the blog index.</summary>
public sealed record PostSummaryResponse(
    string Id,
    string Slug,
    string Title,
    string Excerpt,
    string Locale,
    string AuthorId,
    string AuthorName,
    string? ScientistId,
    DateTime? PublishedAt,
    int CommentCount
);

/// <summary>A full post. Drafts and rejected posts are only returned to their author.</summary>
public sealed record PostResponse(
    string Id,
    string Slug,
    string Title,
    string Excerpt,
    string EditorState,
    string Locale,
    string AuthorId,
    string AuthorName,
    string? ScientistId,
    PostStatus Status,
    string? VerificationFeedback,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? PublishedAt
);

public static class PostMappings
{
    public static PostResponse ToResponse(this Post post) =>
        new(
            post.Id,
            post.Slug,
            post.Title,
            post.Excerpt,
            post.EditorState,
            post.Locale,
            post.AuthorId,
            post.AuthorName,
            post.ScientistId,
            post.Status,
            post.VerificationFeedback,
            post.CreatedAt,
            post.UpdatedAt,
            post.PublishedAt
        );
}
