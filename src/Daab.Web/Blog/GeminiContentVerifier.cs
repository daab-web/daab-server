using System.Text.Json;
using Daab.Modules.Blog.Common;
using Daab.Modules.Translation.Service;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace Daab.Web.Blog;

/// <summary>
/// Host-side bridge: verifies blog posts with the Gemini model the translation module already
/// configures (<see cref="ITranslator"/> is its JSON-mode generation seam). It lives here because
/// the Blog and Translation modules do not reference each other.
/// </summary>
internal sealed class GeminiContentVerifier(
    ITranslator model,
    ILogger<GeminiContentVerifier> logger
) : IContentVerifier
{
    // Keeps prompts (and cost) bounded for very long articles.
    private const int MaxBodyCharacters = 30_000;

    private const string SystemInstruction = """
        You are the content moderator of the blog of an academy of scientists. Verified
        scientists publish articles there for the general public.

        Decide whether the article may be published. Reject it only when it clearly contains any
        of the following:
        - hate speech, harassment, discrimination or personal attacks;
        - sexually explicit content;
        - violence, threats, or encouragement of self-harm or illegal activity;
        - spam, advertising, scams, or links whose only purpose is promotion;
        - other people's private personal data (addresses, phone numbers, ID numbers);
        - dangerous misinformation, especially about health or safety, presented as fact;
        - text that is meaningless, auto-generated filler, or unrelated to its title.

        Opinions, criticism of ideas, controversial but good-faith scientific discussion, and any
        academic topic are acceptable. The article may be written in any language.

        Respond with JSON only, in exactly this shape:
        {"approved": boolean, "reasons": [string]}
        "reasons" is empty when approved. When rejected, give one short, specific and polite
        sentence per problem, addressed to the author, written in the language named by the
        "feedbackLanguage" field of the input.
        """;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public async Task<Fin<ContentVerificationResult>> VerifyAsync(
        ContentVerificationRequest request,
        CancellationToken cancellationToken
    )
    {
        var body =
            request.Body.Length > MaxBodyCharacters
                ? request.Body[..MaxBodyCharacters]
                : request.Body;

        var input = JsonSerializer.Serialize(
            new
            {
                feedbackLanguage = LanguageNames.Of(request.Locale),
                title = request.Title,
                excerpt = request.Excerpt,
                body,
            },
            Json
        );

        var generated = await model.GenerateAsync(SystemInstruction, input, cancellationToken);

        return generated.Bind(Parse);
    }

    private Fin<ContentVerificationResult> Parse(string raw)
    {
        try
        {
            var verdict = JsonSerializer.Deserialize<Verdict>(ModelJson.Unwrap(raw), Json);
            if (verdict?.Approved is not { } approved)
            {
                return FinFail<ContentVerificationResult>(
                    Error.New(502, "Verification model returned no verdict")
                );
            }

            var reasons = (verdict.Reasons ?? [])
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .ToArray();

            return approved
                ? ContentVerificationResult.Approve()
                : ContentVerificationResult.Reject(
                    reasons.Length > 0 ? reasons : ["The content did not pass verification."]
                );
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Unparseable verification response: {Raw}", raw);
            return FinFail<ContentVerificationResult>(
                Error.New(502, "Verification model returned invalid JSON")
            );
        }
    }

    private sealed record Verdict(bool? Approved, List<string>? Reasons);
}
