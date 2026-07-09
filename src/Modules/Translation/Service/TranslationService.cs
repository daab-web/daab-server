using Daab.Modules.Translation.Lexical;
using Daab.Modules.Translation.Memory;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using GlossarySet = Daab.Modules.Translation.Glossary.Glossary;
using IGlossaryProvider = Daab.Modules.Translation.Glossary.IGlossaryProvider;

namespace Daab.Modules.Translation.Service;

/// <summary>
/// Wires Step 1–3 together: extract blocks, serve exact matches from the memory edit cache, and
/// for the rest assemble one prompt (glossary + pooled few-shot examples + numbered tasks), make a
/// single model call, validate the tag multiset per block, and reinsert into the Lexical tree.
/// </summary>
internal sealed class TranslationService(
    ITranslator translator,
    ITranslationMemory memory,
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

    async Task<Fin<TranslationDraft>> TranslateExtractedAsync(
        ExtractResult extract,
        TranslationRequest request,
        CancellationToken cancellationToken
    )
    {
        var blocks = extract.Blocks;
        var translations = new Dictionary<int, string>();
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
                translations[block.Index] = cached;
            }
            else
            {
                misses.Add(block);
            }
        }

        if (misses.Count == 0)
        {
            // Whole document was in cache — no glossary, no examples, no model call.
            return LexicalWalker
                .Reinsert(extract.OriginalJson, translations)
                .Map(json => new TranslationDraft(json, blocks.Count, translations.Count, 0, []));
        }

        var glossary = await SafeGlossaryAsync(request, cancellationToken);
        var examples = await PoolExamplesAsync(misses, request, cancellationToken);

        var (system, user) = PromptBuilder.Build(
            request.ContentType,
            request.SourceLocale,
            request.TargetLocale,
            glossary,
            examples,
            misses
        );

        var generation = await translator.GenerateAsync(system, user, cancellationToken);

        return generation
            .Bind(ModelResponse.ParseArray)
            .Bind(outputs => MapOutputs(outputs, misses, translations))
            .Bind(map => LexicalWalker.Reinsert(extract.OriginalJson, map))
            .Map(json => new TranslationDraft(
                json,
                blocks.Count,
                blocks.Count - misses.Count,
                misses.Count,
                examples
            ));
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

    static Fin<IReadOnlyDictionary<int, string>> MapOutputs(
        IReadOnlyList<string> outputs,
        IReadOnlyList<LexicalBlock> misses,
        Dictionary<int, string> cached
    )
    {
        if (outputs.Count != misses.Count)
        {
            return Error.New(
                502,
                $"Model returned {outputs.Count} translations for {misses.Count} blocks"
            );
        }

        var map = new Dictionary<int, string>(cached);
        var violations = new List<string>();
        for (var i = 0; i < misses.Count; i++)
        {
            var block = misses[i];
            var translated = outputs[i];

            var validation = LexicalWalker.ValidateTags(block.Source, translated);
            if (!validation.Ok)
            {
                violations.Add($"block {block.Index}: {validation.Violation}");
            }

            map[block.Index] = translated;
        }

        // Per-block retry with the error fed back is Step 5; for now this is a hard gate.
        if (violations.Count > 0)
        {
            return Error.New(422, "Tag validation failed: " + string.Join("; ", violations));
        }

        return FinSucc<IReadOnlyDictionary<int, string>>(map);
    }
}
