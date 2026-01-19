namespace Daab.Modules.Scientists.Models;

public abstract  class BaseEntity
{
    public string Id { get; } = Ulid.NewUlid().ToString();
}