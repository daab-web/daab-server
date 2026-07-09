using Daab.Modules.Translation.Memory;
using Daab.Modules.Translation.Models;
using LanguageExt;

namespace Daab.Modules.Translation.Service;

/// <summary>A request to translate one Lexical document end to end.</summary>
/// <param name="EditorStateJson">The Lexical editor-state JSON for the field.</param>
/// <param name="MaxExamples">Upper bound on few-shot pairs injected into the prompt.</param>
/// <param name="MaxTagRetries">
/// How many times a block that fails tag validation is re-prompted with the error fed back before
/// the whole document fails.
/// </param>
public sealed record TranslationRequest(
    string EditorStateJson,
    ContentType ContentType,
    string SourceLocale,
    string TargetLocale,
    int MaxExamples = 5,
    int MaxTagRetries = 2
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

/// <summary>An approved source+target document pair to fold back into the translation memory.</summary>
/// <param name="SourceEditorStateJson">The Lexical editor state that was translated.</param>
/// <param name="TargetEditorStateJson">
/// The approved translation. It must be structurally parallel to the source — same blocks in the
/// same order, each carrying the same inline tags — which is guaranteed for AI-drafted documents.
/// </param>
public sealed record ApprovalRequest(
    string SourceEditorStateJson,
    string TargetEditorStateJson,
    ContentType ContentType,
    string SourceLocale,
    string TargetLocale
);

/// <summary>What an approval wrote back to the memory.</summary>
/// <param name="BlockCount">Translatable blocks in the document.</param>
/// <param name="Added">Newly embedded and stored pairs.</param>
/// <param name="Skipped">Blocks already remembered (same source), left untouched.</param>
public sealed record ApprovalResult(int BlockCount, int Added, int Skipped);

/// <summary>Translates a Lexical document: extract, retrieve, prompt once, validate, reinsert.</summary>
public interface ITranslationService
{
    Task<Fin<TranslationDraft>> TranslateDocumentAsync(
        TranslationRequest request,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Folds an approved source+target document back into the memory: aligns the two documents block
    /// by block, embeds each source block, and stores the pairs — closing the learning loop so future
    /// translations of the same or similar blocks are served from memory. Blocks whose source is
    /// already remembered are skipped.
    /// </summary>
    Task<Fin<ApprovalResult>> ApproveDocumentAsync(
        ApprovalRequest request,
        CancellationToken cancellationToken
    );
}
