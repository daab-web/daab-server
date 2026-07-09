using Daab.Modules.Translation.Memory;
using Daab.Modules.Translation.Models;
using LanguageExt;

namespace Daab.Modules.Translation.Service;

/// <summary>A request to translate one Lexical document end to end.</summary>
/// <param name="EditorStateJson">The Lexical editor-state JSON for the field.</param>
/// <param name="MaxExamples">Upper bound on few-shot pairs injected into the prompt.</param>
public sealed record TranslationRequest(
    string EditorStateJson,
    ContentType ContentType,
    string SourceLocale,
    string TargetLocale,
    int MaxExamples = 5
);

/// <summary>The translated document plus a small accounting of how it was produced.</summary>
/// <param name="ExactHits">Blocks served verbatim from the memory edit cache.</param>
/// <param name="ModelTranslated">Blocks sent to the model.</param>
public sealed record TranslationDraft(
    string TranslatedJson,
    int BlockCount,
    int ExactHits,
    int ModelTranslated,
    IReadOnlyList<TranslationExample> ExamplesUsed
);

/// <summary>Translates a Lexical document: extract, retrieve, prompt once, validate, reinsert.</summary>
public interface ITranslationService
{
    Task<Fin<TranslationDraft>> TranslateDocumentAsync(
        TranslationRequest request,
        CancellationToken cancellationToken
    );
}
