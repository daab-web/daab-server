using Daab.Modules.Translation.Lexical;
using Daab.Modules.Translation.Memory;

namespace Daab.Modules.Translation.Models;

/// <summary>
/// One approved source→target block pair. The source/target are the TAGGED block strings produced
/// by <see cref="LexicalWalker"/>; <see cref="Embedding"/> is filled once at approval time.
/// </summary>
public sealed class TranslationMemoryEntry
{
    public string Id { get; init; } = Ulid.NewUlid().ToString();
    public required string SourceHash { get; init; }
    public required string SourceText { get; init; }
    public required string TargetText { get; init; }
    public required ContentType ContentType { get; init; }
    public required BlockType BlockType { get; init; }
    public required string SourceLocale { get; init; }
    public required string TargetLocale { get; init; }
    public float[]? Embedding { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public static TranslationMemoryEntry Create(
        string taggedSource,
        string taggedTarget,
        ContentType contentType,
        BlockType blockType,
        string sourceLocale,
        string targetLocale,
        float[]? embedding = null
    ) =>
        new()
        {
            SourceHash = SourceKey.Hash(taggedSource),
            SourceText = taggedSource,
            TargetText = taggedTarget,
            ContentType = contentType,
            BlockType = blockType,
            SourceLocale = sourceLocale,
            TargetLocale = targetLocale,
            Embedding = embedding,
        };
}
