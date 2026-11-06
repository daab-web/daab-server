namespace Daab.Modules.Blog.Models;

public class Comment
{
    public string Id { get; private init; } = Ulid.NewUlid().ToString();
    public string PostId { get; private init; } = string.Empty;
    public Post Post { get; private init; } = null!;

    public string AuthorId { get; private init; } = string.Empty;
    public string AuthorName { get; private init; } = string.Empty;

    public string Content { get; private init; } = string.Empty;

    /// <summary>
    /// ISO 639-1 code of the language the comment is written in, or <c>null</c> when it could not
    /// be detected.
    /// </summary>
    public string? Language { get; private set; }

    public DateTime CreatedAt { get; private init; } = DateTime.UtcNow;

    public ICollection<CommentTranslation> Translations { get; private init; } = [];

    private Comment() { }

    public static Comment Create(
        string postId,
        string authorId,
        string authorName,
        string content,
        string? language
    ) =>
        new()
        {
            PostId = postId,
            AuthorId = authorId,
            AuthorName = authorName,
            Content = content.Trim(),
            Language = language,
        };

    public bool IsOwnedBy(string userId) => AuthorId == userId;
}

/// <summary>Cached machine translation of a comment into one locale.</summary>
public class CommentTranslation
{
    public required string CommentId { get; init; }
    public Comment Comment { get; init; } = null!;
    public required string Locale { get; init; }
    public required string Content { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
