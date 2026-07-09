using LanguageExt;

namespace Daab.Modules.Translation.Service;

/// <summary>
/// The single generation call the service depends on: a stable system instruction plus the
/// per-document user content in, the model's raw text out (expected to be the JSON array the
/// prompt asks for). Kept as a seam so the orchestration is unit-testable without Gemini.
/// </summary>
public interface ITranslator
{
    Task<Fin<string>> GenerateAsync(
        string systemInstruction,
        string userContent,
        CancellationToken cancellationToken
    );
}
