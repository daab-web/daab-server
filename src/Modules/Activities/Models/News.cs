namespace Daab.Modules.Activities.Models;

public class News
{
    public string Id { get; } = Ulid.NewUlid().ToString();
    public string? AuthorId { get; init; }
    public required string Title { get; set; }
    public required string Thumbnail { get; set; }
    public string Content { get; set; } = string.Empty;
}