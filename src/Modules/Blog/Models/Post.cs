using Daab.SharedKernel;

namespace Daab.Modules.Blog.Models;

/// <summary>The content of a post in one language.</summary>
/// <param name="EditorState">Serialized Lexical editor state.</param>
public sealed record PostContent(string Locale, string Title, string Excerpt, string EditorState);

public class Post
{
    public string Id { get; private init; } = Ulid.NewUlid().ToString();
    public string Slug { get; private init; } = string.Empty;

    public string AuthorId { get; private init; } = string.Empty;
    public string AuthorName { get; private set; } = string.Empty;

    /// <summary>The author's scientist profile, used to link back to it.</summary>
    public string? ScientistId { get; private set; }

    /// <summary>The language the author wrote the post in; the others are translations.</summary>
    public string SourceLocale { get; private set; } = string.Empty;

    /// <summary>The post in every supported language.</summary>
    public ICollection<PostTranslation> Translations { get; private init; } = [];

    public PostStatus Status { get; private set; } = PostStatus.Draft;

    /// <summary>Why verification rejected the post, shown to its author.</summary>
    public string? VerificationFeedback { get; private set; }

    public DateTime CreatedAt { get; private init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? VerifiedAt { get; private set; }
    public DateTime? PublishedAt { get; private set; }

    public ICollection<Comment> Comments { get; private init; } = [];

    private Post() { }

    public static Post Create(
        string authorId,
        string authorName,
        string? scientistId,
        string sourceLocale,
        IReadOnlyCollection<PostContent> contents
    )
    {
        var id = Ulid.NewUlid().ToString();
        var source = contents.First(c => c.Locale == sourceLocale);
        var slug = SlugHelper.GenerateSlug(source.Title);
        var suffix = id[^6..].ToLowerInvariant();

        var post = new Post
        {
            Id = id,
            Slug = string.IsNullOrEmpty(slug) ? suffix : $"{slug}-{suffix}",
            AuthorId = authorId,
            AuthorName = authorName,
            ScientistId = scientistId,
            SourceLocale = sourceLocale,
        };
        post.ReplaceContents(contents);
        return post;
    }

    public bool IsOwnedBy(string userId) => AuthorId == userId;

    /// <summary>The post in <paramref name="locale"/>, falling back to the original language.</summary>
    public PostTranslation? In(string? locale) =>
        Translations.FirstOrDefault(t => t.Locale == locale)
        ?? Translations.FirstOrDefault(t => t.Locale == SourceLocale)
        ?? Translations.FirstOrDefault();

    /// <summary>
    /// Any edit sends the post back to draft: published content must always be the content that
    /// passed verification.
    /// </summary>
    public void Update(string sourceLocale, IReadOnlyCollection<PostContent> contents)
    {
        SourceLocale = sourceLocale;
        ReplaceContents(contents);
        UpdatedAt = DateTime.UtcNow;
        Status = PostStatus.Draft;
        VerificationFeedback = null;
        PublishedAt = null;
    }

    public void ApplyVerification(bool approved, string? feedback)
    {
        var now = DateTime.UtcNow;
        VerifiedAt = now;

        if (approved)
        {
            Status = PostStatus.Published;
            PublishedAt = now;
            VerificationFeedback = null;
        }
        else
        {
            Status = PostStatus.Rejected;
            PublishedAt = null;
            VerificationFeedback = feedback;
        }
    }

    private void ReplaceContents(IReadOnlyCollection<PostContent> contents)
    {
        foreach (var content in contents)
        {
            var existing = Translations.FirstOrDefault(t => t.Locale == content.Locale);
            if (existing is null)
            {
                Translations.Add(
                    new PostTranslation
                    {
                        PostId = Id,
                        Locale = content.Locale,
                        Title = content.Title.Trim(),
                        Excerpt = content.Excerpt.Trim(),
                        EditorState = content.EditorState,
                    }
                );
            }
            else
            {
                existing.Update(content);
            }
        }

        foreach (
            var stale in Translations.Where(t => contents.All(c => c.Locale != t.Locale)).ToList()
        )
        {
            Translations.Remove(stale);
        }
    }
}

/// <summary>A post's title, summary and body in one language.</summary>
public class PostTranslation
{
    public required string PostId { get; init; }
    public Post Post { get; init; } = null!;
    public required string Locale { get; init; }

    public required string Title { get; set; }
    public string Excerpt { get; set; } = string.Empty;

    /// <summary>Serialized Lexical editor state.</summary>
    public string EditorState { get; set; } = string.Empty;

    public void Update(PostContent content)
    {
        Title = content.Title.Trim();
        Excerpt = content.Excerpt.Trim();
        EditorState = content.EditorState;
    }
}
