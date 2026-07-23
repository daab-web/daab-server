using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Daab.Modules.Translation.Memory;
using Daab.Modules.Translation.Service;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Options;
using static LanguageExt.Prelude;

namespace Daab.Modules.Translation.Gemini;

/// <summary>
/// Typed <see cref="HttpClient"/> over the Gemini REST API. Implements both seams the module
/// needs: <see cref="ITranslator"/> (generateContent, JSON-mode) and <see cref="IEmbedder"/>
/// (embedContent). Base address and the <c>x-goog-api-key</c> header are configured at
/// registration; see <c>DependencyInjection</c>.
/// </summary>
internal sealed class GeminiClient(HttpClient http, IOptions<GeminiOptions> options)
    : ITranslator,
        IEmbedder
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public async Task<Fin<string>> GenerateAsync(
        string systemInstruction,
        string userContent,
        CancellationToken cancellationToken
    )
    {
        var config = options.Value;
        var request = new GeminiGenerateRequest(
            SystemInstruction: new GeminiContent([new GeminiPart(systemInstruction)]),
            Contents: [new GeminiContent([new GeminiPart(userContent)], "user")],
            GenerationConfig: new GeminiGenerationConfig("application/json", config.Temperature)
        );

        var result = await PostAsync<GeminiGenerateRequest, GeminiGenerateResponse>(
            $"models/{config.GenerationModel}:generateContent",
            request,
            cancellationToken
        );

        return result.Match(
            Succ: response =>
            {
                var text = response
                    .Candidates?.FirstOrDefault()
                    ?.Content?.Parts?.FirstOrDefault()
                    ?.Text;
                return string.IsNullOrWhiteSpace(text)
                    ? FinFail<string>(Error.New(502, "Gemini returned an empty generation"))
                    : FinSucc(text);
            },
            Fail: FinFail<string>
        );
    }

    public async Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken)
    {
        var config = options.Value;
        var request = new GeminiEmbedRequest(
            $"models/{config.EmbeddingModel}",
            new GeminiContent([new GeminiPart(text)])
        );

        var result = await PostAsync<GeminiEmbedRequest, GeminiEmbedResponse>(
            $"models/{config.EmbeddingModel}:embedContent",
            request,
            cancellationToken
        );

        // IEmbedder is non-Fin by contract; surface failure as an exception the memory tier
        // catches and turns back into a Fin.
        return result.Match(
            Succ: response =>
                response.Embedding?.Values is { Count: > 0 } values
                    ? values.ToArray()
                    : throw new InvalidOperationException("Gemini returned an empty embedding"),
            Fail: error => throw new InvalidOperationException(error.Message)
        );
    }

    async Task<Fin<TResponse>> PostAsync<TRequest, TResponse>(
        string path,
        TRequest body,
        CancellationToken cancellationToken
    )
    {
        try
        {
            using var response = await http.PostAsJsonAsync(path, body, Json, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var detail = await response.Content.ReadAsStringAsync(cancellationToken);
                return Error.New(
                    (int)response.StatusCode,
                    $"Gemini {path} failed ({(int)response.StatusCode}): {Truncate(detail)}"
                );
            }

            var parsed = await response.Content.ReadFromJsonAsync<TResponse>(
                Json,
                cancellationToken
            );
            if (parsed is null)
            {
                return Error.New(502, $"Gemini {path} returned no body");
            }

            return FinSucc(parsed);
        }
        catch (Exception ex)
            when (ex is HttpRequestException or JsonException
                || (ex is TaskCanceledException && !cancellationToken.IsCancellationRequested)
            )
        {
            return Error.New(502, $"Gemini {path} error: {ex.Message}");
        }
    }

    static string Truncate(string value) => value.Length <= 500 ? value : value[..500];
}
