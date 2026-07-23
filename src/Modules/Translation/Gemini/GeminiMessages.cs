namespace Daab.Modules.Translation.Gemini;

// Minimal DTOs for the Gemini generateContent / embedContent REST shapes. Serialized with the
// web (camelCase) policy, so `SystemInstruction` → `systemInstruction`, etc. Null fields are
// omitted on write.

internal sealed record GeminiPart(string Text);

internal sealed record GeminiContent(IReadOnlyList<GeminiPart> Parts, string? Role = null);

internal sealed record GeminiGenerationConfig(string ResponseMimeType, double Temperature);

internal sealed record GeminiGenerateRequest(
    GeminiContent SystemInstruction,
    IReadOnlyList<GeminiContent> Contents,
    GeminiGenerationConfig GenerationConfig
);

internal sealed record GeminiCandidate(GeminiContent? Content, string? FinishReason);

internal sealed record GeminiGenerateResponse(IReadOnlyList<GeminiCandidate>? Candidates);

internal sealed record GeminiEmbedRequest(string Model, GeminiContent Content);

internal sealed record GeminiEmbedding(IReadOnlyList<float>? Values);

internal sealed record GeminiEmbedResponse(GeminiEmbedding? Embedding);
