using System.Text.Json;
using System.Text.RegularExpressions;
using Daab.Modules.Blog.Common;
using Daab.Modules.Translation.Service;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace Daab.Web.Blog;

/// <summary>
/// Host-side bridge: detects and translates blog comments with the translation module's Gemini
/// generation seam.
/// </summary>
internal sealed partial class GeminiCommentLanguageService(ITranslator model)
    : ICommentLanguageService
{
    private const string DetectInstruction = """
        Identify the language of the user's text. Respond with JSON only:
        {"language": "<ISO 639-1 code in lowercase, e.g. en, az, ru, tr>"}
        If the text mixes languages, use the dominant one. If it has no words (only emoji,
        numbers or links), use "und".
        """;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public async Task<Fin<string>> DetectLanguageAsync(
        string text,
        CancellationToken cancellationToken
    )
    {
        var generated = await model.GenerateAsync(DetectInstruction, text, cancellationToken);

        return generated.Bind(raw =>
            Read<DetectResult>(raw)
                .Bind(r =>
                    r.Language is { } code && IsoCode().IsMatch(code.Trim().ToLowerInvariant())
                        ? FinSucc(code.Trim().ToLowerInvariant())
                        : FinFail<string>(Error.New(502, "Model returned no language code"))
                )
        );
    }

    public async Task<Fin<string>> TranslateAsync(
        string text,
        string targetLocale,
        CancellationToken cancellationToken
    )
    {
        var instruction = $$"""
            Translate the user's text, a comment on a blog article, into {{LanguageNames.Of(
                targetLocale
            )}}.
            Keep its meaning, tone, line breaks, names, links and emoji. Do not add, explain or
            censor anything. If it is already in that language, return it unchanged.
            Respond with JSON only: {"translation": "<translated text>"}
            """;

        var generated = await model.GenerateAsync(instruction, text, cancellationToken);

        return generated.Bind(raw =>
            Read<TranslateResult>(raw)
                .Bind(r =>
                    string.IsNullOrWhiteSpace(r.Translation)
                        ? FinFail<string>(Error.New(502, "Model returned an empty translation"))
                        : FinSucc(r.Translation.Trim())
                )
        );
    }

    private static Fin<T> Read<T>(string raw)
    {
        try
        {
            var value = JsonSerializer.Deserialize<T>(ModelJson.Unwrap(raw), Json);
            return value is null
                ? FinFail<T>(Error.New(502, "Model returned an empty response"))
                : FinSucc(value);
        }
        catch (JsonException)
        {
            return FinFail<T>(Error.New(502, "Model returned invalid JSON"));
        }
    }

    [GeneratedRegex("^([a-z]{2,3})$")]
    private static partial Regex IsoCode();

    private sealed record DetectResult(string? Language);

    private sealed record TranslateResult(string? Translation);
}
