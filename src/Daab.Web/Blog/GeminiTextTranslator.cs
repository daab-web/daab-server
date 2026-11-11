using System.Text.Json;
using Daab.Modules.Blog.Common;
using Daab.Modules.Translation.Service;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace Daab.Web.Blog;

/// <summary>
/// Host-side bridge: suggests translations of plain blog texts (post titles and summaries,
/// comments) with the translation module's Gemini generation seam. Post bodies (Lexical) go
/// through the translation module's own <c>/translations/draft</c> pipeline instead.
/// </summary>
internal sealed class GeminiTextTranslator(ITranslator model) : ITextTranslator
{
    private const string Instruction = """
        Translate each text in the "texts" array of the user's JSON from the "source" language into
        the "target" language. The texts come from a blog of an academy of scientists: article
        titles, short summaries and readers' comments.

        Keep the meaning, tone, line breaks, names, numbers, links and emoji. Do not add, explain,
        summarize or censor anything. If a text is already in the target language, return it
        unchanged.

        Respond with JSON only: {"translations": ["...", "..."]}, with exactly one translation per
        input text, in the same order.
        """;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public async Task<Fin<IReadOnlyList<string>>> TranslateAsync(
        IReadOnlyList<string> texts,
        string sourceLocale,
        string targetLocale,
        CancellationToken cancellationToken
    )
    {
        var input = JsonSerializer.Serialize(
            new
            {
                source = LanguageNames.Of(sourceLocale),
                target = LanguageNames.Of(targetLocale),
                texts,
            },
            Json
        );

        var generated = await model.GenerateAsync(Instruction, input, cancellationToken);

        return generated.Bind(raw => Parse(raw, texts.Count));
    }

    private static Fin<IReadOnlyList<string>> Parse(string raw, int expected)
    {
        try
        {
            var result = JsonSerializer.Deserialize<TranslateResult>(ModelJson.Unwrap(raw), Json);
            var translations = result?.Translations;

            return translations is { Count: > 0 } && translations.Count == expected
                ? FinSucc<IReadOnlyList<string>>([.. translations.Select(t => t?.Trim() ?? "")])
                : FinFail<IReadOnlyList<string>>(
                    Error.New(502, "Model returned the wrong number of translations")
                );
        }
        catch (JsonException)
        {
            return FinFail<IReadOnlyList<string>>(Error.New(502, "Model returned invalid JSON"));
        }
    }

    private sealed record TranslateResult(List<string?>? Translations);
}
