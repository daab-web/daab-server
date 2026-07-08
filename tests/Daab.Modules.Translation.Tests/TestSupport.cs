using System.Reflection;
using System.Text.Json.Nodes;
using LanguageExt;
using LanguageExt.Common;

namespace Daab.Modules.Translation.Tests;

internal static class FinExtensions
{
    public static T ExpectSucc<T>(this Fin<T> fin) =>
        fin.Match(
            value => value,
            error =>
                throw new InvalidOperationException(
                    $"Expected success, got failure: {error.Message}"
                )
        );

    public static Error ExpectFail<T>(this Fin<T> fin) =>
        fin.Match(
            _ => throw new InvalidOperationException("Expected failure, got success"),
            error => error
        );
}

internal static class Fixtures
{
    public const string ScientistAz = "scientist_az.json";
    public const string ScientistEn = "scientist_en.json";
    public const string NewsAz = "news_az.json";
    public const string NewsEn = "news_en.json";

    public static string Load(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resource =
            Array.Find(
                assembly.GetManifestResourceNames(),
                r => r.EndsWith(name, StringComparison.Ordinal)
            ) ?? throw new InvalidOperationException($"Embedded fixture not found: {name}");
        using var stream = assembly.GetManifestResourceStream(resource)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}

/// <summary>Builders for small, hand-made Lexical documents used by the synthetic tests.</summary>
internal static class LexicalDocs
{
    public static string Doc(params JsonObject[] blocks) =>
        new JsonObject
        {
            ["root"] = new JsonObject
            {
                ["children"] = new JsonArray(blocks),
                ["direction"] = null,
                ["format"] = "",
                ["indent"] = 0,
                ["type"] = "root",
                ["version"] = 1,
            },
        }.ToJsonString();

    public static JsonObject Text(string text, int format = 0) =>
        new()
        {
            ["detail"] = 0,
            ["format"] = format,
            ["mode"] = "normal",
            ["style"] = "",
            ["text"] = text,
            ["type"] = "text",
            ["version"] = 1,
        };

    public static JsonObject Paragraph(params JsonNode[] children) =>
        new()
        {
            ["children"] = new JsonArray(children),
            ["direction"] = null,
            ["format"] = "",
            ["indent"] = 0,
            ["type"] = "paragraph",
            ["version"] = 1,
            ["textFormat"] = 0,
            ["textStyle"] = "",
        };

    public static JsonObject Link(string anchor, string url) =>
        new()
        {
            ["children"] = new JsonArray(Text(anchor)),
            ["direction"] = null,
            ["format"] = "",
            ["indent"] = 0,
            ["type"] = "link",
            ["version"] = 1,
            ["rel"] = "noopener noreferrer",
            ["target"] = "_blank",
            ["title"] = null,
            ["url"] = url,
        };

    /// <summary>Returns the <c>children</c> array of the block reached by following a node path.</summary>
    public static JsonArray Children(string json, params int[] path)
    {
        var node = JsonNode.Parse(json)!["root"]!;
        foreach (int index in path)
        {
            node = node["children"]![index]!;
        }
        return node["children"]!.AsArray();
    }
}
