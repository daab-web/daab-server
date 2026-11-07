using Daab.Modules.Blog.Models;

namespace Daab.Modules.Blog.Features.Posts.GetMyPosts;

/// <param name="Title">The title in the original language.</param>
/// <param name="Locales">The languages the post is available in.</param>
public sealed record MyPostResponse(
    string Id,
    string Slug,
    string Title,
    string SourceLocale,
    IReadOnlyList<string> Locales,
    PostStatus Status,
    string? VerificationFeedback,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? PublishedAt,
    int CommentCount
);
