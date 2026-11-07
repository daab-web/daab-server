namespace Daab.Modules.Blog.Models;

public class Comment
{
    public string Id { get; private init; } = Ulid.NewUlid().ToString();
    public string PostId { get; private init; } = string.Empty;
    public Post Post { get; private init; } = null!;

    public string AuthorId { get; private init; } = string.Empty;
    public string AuthorName { get; private init; } = string.Empty;

    /// <summary>The language the commenter wrote in; the others are translations.</summary>
    public string SourceLocale { get; private init; } = string.Empty;

    /// <summary>The comment in every supported language.</summary>
    public ICollection<CommentText> Texts { get; private init; } = [];

    public DateTime CreatedAt { get; private init; } = DateTime.UtcNow;

    private Comment() { }

    public static Comment Create(
        string postId,
        string authorId,
        string authorName,
        string sourceLocale,
        IReadOnlyDictionary<string, string> textsByLocale
    )
    {
        var comment = new Comment
        {
            PostId = postId,
            AuthorId = authorId,
            AuthorName = authorName,
            SourceLocale = sourceLocale,
        };

        foreach (var (locale, content) in textsByLocale)
        {
            comment.Texts.Add(
                new CommentText
                {
                    CommentId = comment.Id,
                    Locale = locale,
                    Content = content.Trim(),
                }
            );
        }

        return comment;
    }

    public bool IsOwnedBy(string userId) => AuthorId == userId;
}

/// <summary>A comment's text in one language.</summary>
public class CommentText
{
    public required string CommentId { get; init; }
    public Comment Comment { get; init; } = null!;
    public required string Locale { get; init; }
    public required string Content { get; init; }
}
