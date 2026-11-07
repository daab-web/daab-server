using Daab.Modules.Blog.Models;

namespace Daab.Modules.Blog.Features.Posts.GetMyPosts;

public sealed record MyPostResponse(
    string Id,
    string Slug,
    string Title,
    string Excerpt,
    string Locale,
    PostStatus Status,
    string? VerificationFeedback,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? PublishedAt,
    int CommentCount
);
