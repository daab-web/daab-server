using System.Text.Json;
using Daab.Modules.Translation.Service;
using LanguageExt;
using LanguageExt.Common;

namespace Daab.Modules.Translation.Tests.Service;

/// <summary>Records the prompt it was handed and returns a canned response.</summary>
internal sealed class FakeTranslator(Func<string, string, Fin<string>> handler) : ITranslator
{
    public int CallCount { get; private set; }
    public string? LastSystem { get; private set; }
    public string? LastUser { get; private set; }

    /// <summary>Responds with the given strings serialized as the JSON array the service expects.</summary>
    public static FakeTranslator Returning(params string[] outputs) =>
        new((_, _) => (Fin<string>)JsonSerializer.Serialize(outputs));

    /// <summary>Responds with a verbatim (possibly malformed) body.</summary>
    public static FakeTranslator Raw(string response) => new((_, _) => (Fin<string>)response);

    /// <summary>Responds with a different JSON array per successive call (drives retry tests).</summary>
    public static FakeTranslator Sequence(params string[][] outputsPerCall)
    {
        var queue = new Queue<string[]>(outputsPerCall);
        return new FakeTranslator(
            (_, _) =>
                queue.Count > 0
                    ? (Fin<string>)JsonSerializer.Serialize(queue.Dequeue())
                    : (Fin<string>)Error.New(502, "FakeTranslator: no more responses")
        );
    }

    public Task<Fin<string>> GenerateAsync(
        string systemInstruction,
        string userContent,
        CancellationToken cancellationToken
    )
    {
        CallCount++;
        LastSystem = systemInstruction;
        LastUser = userContent;
        return Task.FromResult(handler(systemInstruction, userContent));
    }
}
