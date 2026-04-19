namespace Daab.Modules.Scientists.Models;

public class Director
{
    public string Id { get; init; } = Ulid.NewUlid().ToString();
    public string Role { get; set; } = string.Empty;

    public required string ScientistId { get; set; }
    public Scientist Scientist { get; set; } = null!;
}
