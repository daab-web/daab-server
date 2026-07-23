using System.Text.Json;
using System.Text.Json.Nodes;

namespace Daab.Modules.Translation.Lexical;

/// <summary>Low-level helpers for building and navigating Lexical node trees.</summary>
internal static class LexicalTree
{
    /// <summary>Builds a Lexical text node, emitting keys in the order Lexical itself uses.</summary>
    public static JsonObject MakeText(
        int format,
        int detail,
        string mode,
        string style,
        string text
    ) =>
        new()
        {
            ["detail"] = detail,
            ["format"] = format,
            ["mode"] = mode,
            ["style"] = style,
            ["text"] = text,
            ["type"] = "text",
            ["version"] = 1,
        };

    public static JsonObject MakeLineBreak() => new() { ["type"] = "linebreak", ["version"] = 1 };

    /// <summary>Follows a node path from the document root to the addressed block node.</summary>
    public static JsonNode Navigate(JsonNode root, IReadOnlyList<int> path)
    {
        var current = root["root"] ?? throw new JsonException("Missing 'root' node");
        foreach (int index in path)
        {
            var children =
                current["children"]?.AsArray()
                ?? throw new JsonException("Missing 'children' during navigation");
            if (index < 0 || index >= children.Count)
            {
                throw new JsonException("Node path out of range");
            }
            current = children[index] ?? throw new JsonException("Null child during navigation");
        }
        return current;
    }

    /// <summary>
    /// Collapses consecutive text nodes that share formatting into a single node. Lexical renders
    /// them identically, so this is lossless; it lets the round-trip gate ignore the harmless way a
    /// rebuilt tree may merge adjacent same-format runs.
    /// </summary>
    public static void Canonicalize(JsonNode? node)
    {
        switch (node)
        {
            case JsonObject obj:
                if (obj["children"] is JsonArray children)
                {
                    CoalesceAdjacentText(children);
                }
                foreach (var pair in obj)
                {
                    Canonicalize(pair.Value);
                }
                break;
            case JsonArray array:
                foreach (var item in array)
                {
                    Canonicalize(item);
                }
                break;
        }
    }

    static void CoalesceAdjacentText(JsonArray children)
    {
        for (int i = children.Count - 1; i > 0; i--)
        {
            if (
                children[i] is JsonObject current
                && children[i - 1] is JsonObject previous
                && IsText(current)
                && IsText(previous)
                && SameFormat(previous, current)
            )
            {
                string merged =
                    (previous["text"]?.GetValue<string>() ?? "")
                    + (current["text"]?.GetValue<string>() ?? "");
                previous["text"] = merged;
                children.RemoveAt(i);
            }
        }
    }

    static bool IsText(JsonObject node) => node["type"]?.GetValue<string>() == "text";

    static bool SameFormat(JsonObject a, JsonObject b) =>
        JsonNode.DeepEquals(a["format"], b["format"])
        && JsonNode.DeepEquals(a["detail"], b["detail"])
        && JsonNode.DeepEquals(a["mode"], b["mode"])
        && JsonNode.DeepEquals(a["style"], b["style"])
        && JsonNode.DeepEquals(a["version"], b["version"]);
}
