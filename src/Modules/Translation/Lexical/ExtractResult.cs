namespace Daab.Modules.Translation.Lexical;

/// <summary>The outcome of extracting translatable blocks from a Lexical editor state.</summary>
public sealed record ExtractResult(string OriginalJson, IReadOnlyList<LexicalBlock> Blocks);

/// <summary>
/// Result of checking that a translated block preserves the source block's tag multiset.
/// </summary>
public sealed record TagValidation(bool Ok, string? Violation);
