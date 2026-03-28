namespace Daab.SharedKernel.Entities;

public class Locale
{
    public string Id { get; init; } = Ulid.NewUlid().ToString();
    public required string Name { get; init; }
}
