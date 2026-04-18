using Daab.SharedKernel.Entities;

namespace Daab.Modules.Activities.Models;

public class News
{
    public string Id { get; } = Ulid.NewUlid().ToString();
    public required string Title { get; set; }
    public required string Slug { get; init; }
    public string? Thumbnail { get; set; }
    public string? Excerpt { get; set; }
    public required DateTime PublishedDate { get; init; }

    public string? AuthorId { get; init; }
    public string? AuthorName { get; init; }

    public string? Category { get; set; }
    public string EditorState { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];

    public ICollection<Attachment> Attachments { get; set; } = [];

    public ICollection<NewsTranslation> Translations { get; set; } = [];
}

public class NewsTranslation
{
    public required string NewsId { get; init; }
    public News News { get; set; } = null!;
    public required string Locale { get; init; }
    public TranslationStatus Status { get; private set; } = TranslationStatus.Untranslated;

    public string? Title { get; private set; }
    public string? Excerpt { get; private set; }
    public string? EditorState { get; private set; }

    public static NewsTranslation Create(
        string newsId,
        string locale,
        string title,
        string excerpt,
        string editorState
    )
    {
        return new NewsTranslation
        {
            NewsId = newsId,
            Locale = locale,
            Title = title,
            Excerpt = excerpt,
            EditorState = editorState,
        };
    }

    public void Update(string title, string excerpt, string editorState)
    {
        Title = title;
        Excerpt = excerpt;
        EditorState = editorState;

        Status = TranslationStatus.Translated;
    }
}
