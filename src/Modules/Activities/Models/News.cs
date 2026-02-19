namespace Daab.Modules.Activities.Models;

public class News
{
    public string Id { get; } = Ulid.NewUlid().ToString();
    public required string Title { get; set; }
    public required string Slug { get; init; }
    public string? Thumbnail { get; set; }
    public string? Excerpt { get; init; }
    public required DateTimeOffset PublishedDate { get; init; }

    public string? AuthorId { get; init; }
    public string? AuthorName { get; init; }

    public string? Category { get; init; }
    public string EditorState { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
}
