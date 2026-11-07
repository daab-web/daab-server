using Daab.SharedKernel.Middlewares;

namespace Daab.Modules.Blog.Features.Posts.UpdatePost;

public sealed record UpdatePostRequest(
    string Id,
    string Title,
    string Excerpt,
    string EditorState,
    string Locale
) : IPostContent, ILocalizedCollection
{
    public IEnumerable<string> GetLocales() => [Locale];
}
