using Daab.Modules.Translation.Models;
using Daab.Modules.Translation.Persistence;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Translation.Memory;

internal sealed class TranslationMemory(TranslationDbContext context, IEmbedder embedder)
    : ITranslationMemory
{
    public async Task<string?> FindExactAsync(
        TranslationLookup key,
        CancellationToken cancellationToken
    )
    {
        var hash = SourceKey.Hash(key.SourceText);
        return await context
            .Entries.Where(e =>
                e.SourceHash == hash
                && e.ContentType == key.ContentType
                && e.BlockType == key.BlockType
                && e.SourceLocale == key.SourceLocale
                && e.TargetLocale == key.TargetLocale
            )
            .Select(e => e.TargetText)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Fin<IReadOnlyList<TranslationExample>>> GetExamplesAsync(
        TranslationQuery query,
        CancellationToken cancellationToken
    )
    {
        var examples = new List<TranslationExample>();

        var incomingHash = SourceKey.Hash(query.SourceText);
        var candidates = await context
            .Entries.Where(e =>
                e.ContentType == query.ContentType
                && e.BlockType == query.BlockType
                && e.SourceLocale == query.SourceLocale
                && e.TargetLocale == query.TargetLocale
                && e.SourceHash != incomingHash
            )
            .ToListAsync(cancellationToken);

        if (candidates.Count == 0)
        {
            return examples;
        }

        // Fuzzy tier: structure-sensitive, so match on the normalized TAGGED strings.
        var normalizedQuery = SourceKey.Normalize(query.SourceText);
        var chosen = new System.Collections.Generic.HashSet<string>();

        var fuzzy = candidates
            .Select(c =>
                (
                    Entry: c,
                    Score: Similarity.NormalizedLevenshtein(
                        normalizedQuery,
                        SourceKey.Normalize(c.SourceText)
                    )
                )
            )
            .Where(x => x.Score >= query.FuzzyThreshold)
            .OrderByDescending(x => x.Score)
            .Take(Math.Min(query.MaxFuzzy, query.K));

        foreach (var (entry, score) in fuzzy)
        {
            examples.Add(
                new TranslationExample(entry.SourceText, entry.TargetText, score, ExampleTier.Fuzzy)
            );
            chosen.Add(entry.Id);
        }

        int remaining = query.K - examples.Count;
        if (remaining <= 0)
        {
            // Fuzzy filled every slot — no need to embed. This short-circuit is the point of tiering.
            return examples;
        }

        var embeddable = candidates
            .Where(c => c.Embedding is not null && !chosen.Contains(c.Id))
            .ToList();
        if (embeddable.Count == 0)
        {
            return examples;
        }

        // Semantic tier: embed the incoming block's PLAIN text once, rank stored vectors with MMR.
        var queryVector = await embedder.EmbedAsync(
            SourceKey.Plain(query.SourceText),
            cancellationToken
        );

        var selected = Similarity.SelectMmr(
            queryVector,
            embeddable,
            remaining,
            query.MmrLambda,
            c => c.Embedding!
        );

        foreach (var entry in selected)
        {
            examples.Add(
                new TranslationExample(
                    entry.SourceText,
                    entry.TargetText,
                    Similarity.CosineSimilarity(queryVector, entry.Embedding!),
                    ExampleTier.Semantic
                )
            );
        }

        return examples;
    }

    public async Task<Fin<string>> AddAsync(
        TranslationMemoryEntry entry,
        CancellationToken cancellationToken
    )
    {
        context.Entries.Add(entry);
        var written = await context.SaveChangesAsync(cancellationToken);
        return written > 0
            ? entry.Id
            : Error.New(500, "Unable to persist translation memory entry");
    }
}
