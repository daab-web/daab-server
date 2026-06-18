namespace Daab.Modules.ReferenceData.Domain;

public class Translation
{
    public string Id { get; } = Ulid.NewUlid().ToString();

    public required string Locale { get; init; }
    public required string Namespace { get; init; }

    public required string Key { get; init; }
    public required string Value { get; set; }
}
