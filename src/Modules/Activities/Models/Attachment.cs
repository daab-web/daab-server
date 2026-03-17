namespace Daab.Modules.Activities.Models;

public class Attachment
{
    public string Id { get; init; } = Ulid.NewUlid().ToString();

    public string FileUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }

    public string? FileType { get; set; }

    public required string ParentObjectId { get; init; }
}
