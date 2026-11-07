namespace Daab.Modules.Blog.Features.Posts.CreatePost;

/// <param name="SourceLocale">The language the author wrote the post in.</param>
/// <param name="Translations">The post in every supported language.</param>
public sealed record CreatePostRequest(string SourceLocale, List<PostTranslationInput> Translations)
    : IPostContent;
