namespace Daab.Modules.Translation.Gemini;

/// <summary>
/// Gemini connection + model settings. <see cref="ApiKey"/> is a secret and lives in
/// user-secrets (<c>Gemini:ApiKey</c>); the rest are non-secret and may sit in appsettings.
/// </summary>
public sealed class GeminiOptions
{
    public const string SectionName = "Gemini";

    /// <summary>API key sent as the <c>x-goog-api-key</c> header. Never commit this.</summary>
    public required string ApiKey { get; init; }

    /// <summary>Base address; must end with a slash so relative model paths resolve.</summary>
    public string BaseUrl { get; init; } = "https://generativelanguage.googleapis.com/v1beta/";

    public string GenerationModel { get; init; } = "gemini-2.5-flash";

    public string EmbeddingModel { get; init; } = "gemini-embedding-001";

    /// <summary>Low temperature keeps translations faithful to the source.</summary>
    public double Temperature { get; init; } = 0.2;
}
