using System.Text.Json;
using Daab.Modules.Translation.Service;
using LanguageExt;

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
