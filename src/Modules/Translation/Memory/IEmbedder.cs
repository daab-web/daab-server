namespace Daab.Modules.Translation.Memory;

/// <summary>Turns a piece of text into a dense embedding vector. Implemented over Gemini in Step 4.</summary>
public interface IEmbedder
{
    Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken);
}
