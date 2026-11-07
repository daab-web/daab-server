using Daab.Modules.Blog.Models;

namespace Daab.Modules.Blog.Features.Posts;

/// <summary>A post's title, summary and body (Lexical editor state) in one language.</summary>
public sealed record PostTranslationDto(
    string Locale,
    string Title,
    string Excerpt,
    string EditorState
);

/// <summary>A published post as listed on the blog index, in the requested language.</summary>
/// <param name="Locale">The language of <see cref="Title"/> and <see cref="Excerpt"/>.</param>
public sealed record PostSummaryResponse(
    string Id,
    string Slug,
    string Locale,
    string Title,
    string Excerpt,
    string AuthorId,
    string AuthorName,
    string? ScientistId,
    DateTime? PublishedAt,
    int CommentCount
);

/// <summary>
/// A full post in every language. Drafts and rejected posts are only returned to their author.
/// </summary>
/// <param name="SourceLocale">The language the author wrote it in.</param>
public sealed record PostResponse(
    string Id,
    string Slug,
    string SourceLocale,
    IReadOnlyList<PostTranslationDto> Translations,
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
            post.SourceLocale,
            [
                .. post
                    .Translations.OrderBy(t => t.Locale == post.SourceLocale ? 0 : 1)
                    .ThenBy(t => t.Locale, StringComparer.Ordinal)
                    .Select(t => new PostTranslationDto(
                        t.Locale,
                        t.Title,
                        t.Excerpt,
                        t.EditorState
                    )),
            ],
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
