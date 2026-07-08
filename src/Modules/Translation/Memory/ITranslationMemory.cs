using Daab.Modules.Translation.Models;
using LanguageExt;

namespace Daab.Modules.Translation.Memory;

/// <summary>The translation memory: an edit cache plus a few-shot example source.</summary>
public interface ITranslationMemory
{
    /// <summary>Exact-hash tier: the stored target for an identical block, or null. No embedding.</summary>
    Task<string?> FindExactAsync(TranslationLookup key, CancellationToken cancellationToken);

    /// <summary>Fuzzy + semantic tiers: up to K MMR-diverse examples for a non-exact block.</summary>
    Task<Fin<IReadOnlyList<TranslationExample>>> GetExamplesAsync(
        TranslationQuery query,
        CancellationToken cancellationToken
    );

    /// <summary>Persists an approved pair; returns its id.</summary>
    Task<Fin<string>> AddAsync(TranslationMemoryEntry entry, CancellationToken cancellationToken);
}
