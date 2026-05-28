using Daab.SharedKernel.Entities;

namespace Daab.Modules.Scientists.Models;

public class Director
{
    public string Id { get; init; } = Ulid.NewUlid().ToString();
    public Dictionary<string, string> RoleTranslations { get; init; } = [];

    public required string ScientistId { get; init; }
    public Scientist Scientist { get; set; } = null!;

    public EntityStatus Status { get; set; } = EntityStatus.Untranslated;
}
