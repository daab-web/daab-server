using Daab.Modules.Translation.Lexical;
using Daab.Modules.Translation.Memory;
using Daab.Modules.Translation.Models;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using GlossarySet = Daab.Modules.Translation.Glossary.Glossary;
using IGlossaryProvider = Daab.Modules.Translation.Glossary.IGlossaryProvider;

namespace Daab.Modules.Translation.Service;

/// <summary>
/// Wires Step 1–3 together: extract blocks, serve exact matches from the memory edit cache, and for
/// the rest assemble one prompt (glossary + pooled few-shot examples + numbered tasks) and call the
/// model. Each translated block must carry the same tag multiset as its source; a block that fails
/// is re-prompted with the error fed back, up to a retry budget. The reinserted document is then
/// round-tripped back through the Lexical schema before being returned.
/// </summary>
internal sealed class TranslationService(
    ITranslator translator,
    ITranslationMemory memory,
    IEmbedder embedder,
    IGlossaryProvider glossaryProvider
) : ITranslationService
{
    public Task<Fin<TranslationDraft>> TranslateDocumentAsync(
        TranslationRequest request,
        CancellationToken cancellationToken
    ) =>
        LexicalWalker
            .Extract(request.EditorStateJson)
            .Match(
                Succ: extract => TranslateExtractedAsync(extract, request, cancellationToken),
                Fail: error => Task.FromResult<Fin<TranslationDraft>>(error)
            );

    public Task<Fin<ApprovalResult>> ApproveDocumentAsync(
        ApprovalRequest request,
        CancellationToken cancellationToken
    )
    {
        var extracted = LexicalWalker
            .Extract(request.SourceEditorStateJson)
            .Bind(source =>
                LexicalWalker.Extract(request.TargetEditorStateJson).Map(target => (source, target))
            );

        return extracted.Match(
            Succ: pair => WriteBackAsync(pair.source, pair.target, request, cancellationToken),
            Fail: error => Task.FromResult<Fin<ApprovalResult>>(error)
        );
    }

    /// <summary>
    /// Align the approved source and target block for block, and for every source block not already
    /// remembered, embed its plain text and store the pair. A structural mismatch (differing block
    /// count or type) or a broken tag multiset rejects the whole approval so the memory stays clean.
    /// </summary>
    async Task<Fin<ApprovalResult>> WriteBackAsync(
        ExtractResult source,
        ExtractResult target,
        ApprovalRequest request,
        CancellationToken cancellationToken
    )
    {
        var sourceBlocks = source.Blocks;
        var targetBlocks = target.Blocks;

        if (sourceBlocks.Count != targetBlocks.Count)
        {
            return Error.New(
                422,
                $"Source has {sourceBlocks.Count} blocks but the approved target has "
                    + $"{targetBlocks.Count}; only structurally parallel documents can be approved"
            );
        }

        var added = 0;
        var skipped = 0;

        for (var i = 0; i < sourceBlocks.Count; i++)
        {
            var sourceBlock = sourceBlocks[i];
            var targetBlock = targetBlocks[i];

            if (sourceBlock.BlockType != targetBlock.BlockType)
            {
                return Error.New(
                    422,
                    $"Block {i} is a {sourceBlock.BlockType} in the source but a "
                        + $"{targetBlock.BlockType} in the target"
                );
            }

            var validation = LexicalWalker.ValidateTags(sourceBlock.Source, targetBlock.Source);
            if (!validation.Ok)
            {
                return Error.New(422, $"Block {i}: {validation.Violation}");
            }

            var existing = await memory.FindExactAsync(
                new TranslationLookup(
                    sourceBlock.Source,
                    request.ContentType,
                    sourceBlock.BlockType,
                    request.SourceLocale,
                    request.TargetLocale
                ),
                cancellationToken
            );
            if (existing is not null)
            {
                skipped++;
                continue;
            }

            float[] embedding;
            try
            {
                embedding = await embedder.EmbedAsync(
                    SourceKey.Plain(sourceBlock.Source),
                    cancellationToken
                );
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Unlike retrieval, approval must not silently store an un-embedded pair: that would
                // quietly disable the semantic tier for it. Fail so the caller can retry.
                return Error.New(502, $"Embedding block {i} for approval failed: {ex.Message}");
            }

            var stored = await memory.AddAsync(
                TranslationMemoryEntry.Create(
                    sourceBlock.Source,
                    targetBlock.Source,
                    request.ContentType,
                    sourceBlock.BlockType,
                    request.SourceLocale,
                    request.TargetLocale,
                    embedding
                ),
                cancellationToken
            );
            if (stored.IsFail)
            {
                return stored.Map(_ => new ApprovalResult(sourceBlocks.Count, added, skipped));
            }

            added++;
        }

        return new ApprovalResult(sourceBlocks.Count, added, skipped);
    }

    async Task<Fin<TranslationDraft>> TranslateExtractedAsync(
        ExtractResult extract,
        TranslationRequest request,
        CancellationToken cancellationToken
    )
    {
        var blocks = extract.Blocks;
        var cache = new Dictionary<int, string>();
        var misses = new List<LexicalBlock>();

        // Exact-hash tier: verbatim from the edit cache, never sent to the model.
        foreach (var block in blocks)
        {
            var cached = await memory.FindExactAsync(
                new TranslationLookup(
                    block.Source,
                    request.ContentType,
                    block.BlockType,
                    request.SourceLocale,
                    request.TargetLocale
                ),
                cancellationToken
            );

            if (cached is not null)
            {
                cache[block.Index] = cached;
            }
            else
            {
                misses.Add(block);
            }
        }

        if (misses.Count == 0)
        {
            // Whole document was in cache — no glossary, no examples, no model call.
            return Reinsert(extract.OriginalJson, cache)
                .Map(json => new TranslationDraft(json, blocks.Count, cache.Count, 0, []));
        }

        var glossary = await SafeGlossaryAsync(request, cancellationToken);
        var examples = await PoolExamplesAsync(misses, request, cancellationToken);

        var translated = await TranslateMissesAsync(
            misses,
            examples,
            glossary,
            request,
            cancellationToken
        );

        return translated
            .Bind(model => Reinsert(extract.OriginalJson, Merge(cache, model)))
            .Map(json => new TranslationDraft(
                json,
                blocks.Count,
                cache.Count,
                misses.Count,
                examples
            ));
    }

    /// <summary>
    /// Translate the miss blocks, re-prompting any that fail tag validation with the error fed back,
    /// until every block validates or the retry budget is spent.
    /// </summary>
    async Task<Fin<IReadOnlyDictionary<int, string>>> TranslateMissesAsync(
        IReadOnlyList<LexicalBlock> misses,
        IReadOnlyList<TranslationExample> examples,
        GlossarySet glossary,
        TranslationRequest request,
        CancellationToken cancellationToken
    )
    {
        var results = new Dictionary<int, string>();
        var pending = misses.ToList();
        var problems = new Dictionary<int, string>();

        for (var attempt = 0; ; attempt++)
        {
            var (system, user) =
                attempt == 0
                    ? PromptBuilder.Build(
                        request.ContentType,
                        request.SourceLocale,
                        request.TargetLocale,
                        glossary,
                        examples,
                        pending
                    )
                    : PromptBuilder.BuildRetry(
                        request.ContentType,
                        request.SourceLocale,
                        request.TargetLocale,
                        glossary,
                        pending,
                        problems
                    );

            var parsed = (await translator.GenerateAsync(system, user, cancellationToken)).Bind(
                ModelResponse.ParseArray
            );
            if (parsed.IsFail)
            {
                // Propagate the model/parse error, retyped to this method's result.
                return parsed.Map(_ => (IReadOnlyDictionary<int, string>)results);
            }

            var outputs = parsed.Match(Succ: o => o, Fail: _ => (IReadOnlyList<string>)[]);
            if (outputs.Count != pending.Count)
            {
                return Error.New(
                    502,
                    $"Model returned {outputs.Count} translations for {pending.Count} blocks"
                );
            }

            var stillPending = new List<LexicalBlock>();
            var nextProblems = new Dictionary<int, string>();
            for (var i = 0; i < pending.Count; i++)
            {
                var block = pending[i];
                var problem = ValidateBlock(block.Source, outputs[i]);
                if (problem is null)
                {
                    results[block.Index] = outputs[i];
                }
                else
                {
                    stillPending.Add(block);
                    nextProblems[block.Index] = problem;
                }
            }

            if (stillPending.Count == 0)
            {
                return FinSucc<IReadOnlyDictionary<int, string>>(results);
            }

            if (attempt >= request.MaxTagRetries)
            {
                var detail = string.Join(
                    "; ",
                    nextProblems.Select(p => $"block {p.Key}: {p.Value}")
                );
                return Error.New(422, $"Tag validation failed after retries: {detail}");
            }

            pending = stillPending;
            problems = nextProblems;
        }
    }

    /// <summary>Pool per-block examples into one deduped, score-ranked set for the whole document.</summary>
    async Task<IReadOnlyList<TranslationExample>> PoolExamplesAsync(
        IReadOnlyList<LexicalBlock> misses,
        TranslationRequest request,
        CancellationToken cancellationToken
    )
    {
        var pooled = new List<TranslationExample>();
        foreach (var block in misses)
        {
            var result = await memory.GetExamplesAsync(
                new TranslationQuery(
                    block.Source,
                    request.ContentType,
                    block.BlockType,
                    request.SourceLocale,
                    request.TargetLocale,
                    K: request.MaxExamples
                ),
                cancellationToken
            );

            // Degrade on failure: examples are an enhancer, the glossary floor still applies.
            result.IfSucc(pooled.AddRange);
        }

        return pooled
            .GroupBy(e => (e.Source, e.Target))
            .Select(g => g.MaxBy(e => e.Score)!)
            .OrderByDescending(e => e.Score)
            .Take(request.MaxExamples)
            .ToList();
    }

    async Task<GlossarySet> SafeGlossaryAsync(
        TranslationRequest request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            return await glossaryProvider.GetGlossaryAsync(
                request.SourceLocale,
                request.TargetLocale,
                cancellationToken
            );
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // The glossary only forces terminology; never fail a document because it is unavailable.
            return GlossarySet.Empty;
        }
    }

    /// <summary>Reinsert translations, then confirm the result still parses as valid Lexical.</summary>
    static Fin<string> Reinsert(
        string originalJson,
        IReadOnlyDictionary<int, string> translations
    ) =>
        LexicalWalker
            .Reinsert(originalJson, translations)
            .Bind(json => LexicalWalker.Extract(json).Map(_ => json));

    static Dictionary<int, string> Merge(
        Dictionary<int, string> cache,
        IReadOnlyDictionary<int, string> model
    )
    {
        var map = new Dictionary<int, string>(cache);
        foreach (var (index, text) in model)
        {
            map[index] = text;
        }

        return map;
    }

    /// <summary>Null when the block is valid, otherwise the reason to feed back to the model.</summary>
    static string? ValidateBlock(string source, string translated)
    {
        if (string.IsNullOrWhiteSpace(translated))
        {
            return "the translation is empty";
        }

        var validation = LexicalWalker.ValidateTags(source, translated);
        return validation.Ok ? null : validation.Violation;
    }
}
