namespace Daab.Modules.Blog.Features.Posts.UpdatePost;

public sealed record UpdatePostRequest(
    string Id,
    string SourceLocale,
    List<PostTranslationInput> Translations
) : IPostContent;
