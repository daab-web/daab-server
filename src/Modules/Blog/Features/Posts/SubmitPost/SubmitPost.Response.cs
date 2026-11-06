using Daab.Modules.Blog.Models;

namespace Daab.Modules.Blog.Features.Posts.SubmitPost;

public sealed record SubmitPostResponse(
    string Id,
    string Slug,
    PostStatus Status,
    string? VerificationFeedback
);

public static class SubmitPostMappings
{
    public static SubmitPostResponse ToSubmitResponse(this Post post) =>
        new(post.Id, post.Slug, post.Status, post.VerificationFeedback);
}
