using Daab.SharedKernel;

namespace Daab.Modules.Blog.Models;

public class Post
{
    public string Id { get; private init; } = Ulid.NewUlid().ToString();
    public string Slug { get; private init; } = string.Empty;

    public string AuthorId { get; private init; } = string.Empty;
    public string AuthorName { get; private set; } = string.Empty;

    /// <summary>The author's scientist profile, used to link back to it.</summary>
    public string? ScientistId { get; private set; }

    public string Title { get; private set; } = string.Empty;
    public string Excerpt { get; private set; } = string.Empty;

    /// <summary>Serialized Lexical editor state.</summary>
    public string EditorState { get; private set; } = string.Empty;

    /// <summary>The language the post is written in.</summary>
    public string Locale { get; private set; } = string.Empty;

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
        string title,
        string excerpt,
        string editorState,
        string locale
    )
    {
        var id = Ulid.NewUlid().ToString();
        var slug = SlugHelper.GenerateSlug(title);
        var suffix = id[^6..].ToLowerInvariant();

        return new Post
        {
            Id = id,
            Slug = string.IsNullOrEmpty(slug) ? suffix : $"{slug}-{suffix}",
            AuthorId = authorId,
            AuthorName = authorName,
            ScientistId = scientistId,
            Title = title.Trim(),
            Excerpt = excerpt.Trim(),
            EditorState = editorState,
            Locale = locale,
        };
    }

    public bool IsOwnedBy(string userId) => AuthorId == userId;

    /// <summary>
    /// Any edit sends the post back to draft: published content must always be the content that
    /// passed verification.
    /// </summary>
    public void Update(string title, string excerpt, string editorState, string locale)
    {
        Title = title.Trim();
        Excerpt = excerpt.Trim();
        EditorState = editorState;
        Locale = locale;
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
}
