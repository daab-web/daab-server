using Daab.SharedKernel.Middlewares;

namespace Daab.Modules.Blog.Features.Posts.CreatePost;

public sealed record CreatePostRequest(
    string Title,
    string Excerpt,
    string EditorState,
    string Locale
) : IPostContent, ILocalizedCollection
{
    public IEnumerable<string> GetLocales() => [Locale];
}
