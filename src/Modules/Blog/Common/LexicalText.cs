using System.Text;
using System.Text.Json;

namespace Daab.Modules.Blog.Common;

/// <summary>Flattens a serialized Lexical editor state into plain text.</summary>
public static class LexicalText
{
    public static bool IsValid(string editorState)
    {
        try
        {
            using var document = JsonDocument.Parse(editorState);
            return document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.TryGetProperty("root", out _);
        }
        catch (JsonException)
        {
            return false;
        }
    }

    public static string Extract(string editorState)
    {
        if (string.IsNullOrWhiteSpace(editorState))
        {
            return string.Empty;
        }

        try
        {
            using var document = JsonDocument.Parse(editorState);
            if (!document.RootElement.TryGetProperty("root", out var root))
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            Walk(root, builder);
            return builder.ToString().Trim();
        }
        catch (JsonException)
        {
            return string.Empty;
        }
    }

    private static void Walk(JsonElement node, StringBuilder builder)
    {
        if (node.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        if (node.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String)
        {
            builder.Append(text.GetString());
        }

        if (
            node.TryGetProperty("children", out var children)
            && children.ValueKind == JsonValueKind.Array
        )
        {
            foreach (var child in children.EnumerateArray())
            {
                Walk(child, builder);
            }

            // Element nodes (paragraphs, headings, list items...) end a line.
            builder.Append('\n');
        }
    }
}
