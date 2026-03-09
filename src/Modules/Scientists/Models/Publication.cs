namespace Daab.Modules.Scientists.Models;

public class Publication
{
    public Publication(string? url)
    {
        Url = url;
    }

    public string Id { get; set; } = Ulid.NewUlid().ToString();
    public required string Title { get; set; }
    public string? Url { get; set; }

    public string ScientistId { get; set; } = null!;
    public Scientist Scientist { get; set; } = null!;
}
