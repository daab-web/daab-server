using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using LanguageExt;
using LanguageExt.Common;

namespace Daab.Modules.Translation.Lexical;

/// <summary>
/// Extracts translatable text from a Lexical <c>SerializedEditorState</c> and splices translations
/// back into a clone of the original tree. Structure, formatting and link attributes are preserved
/// by construction: the tree is never handed to a model, only per-block inline text is.
/// </summary>
public static class LexicalWalker
{
    static readonly JsonSerializerOptions RelaxedOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>Flattens each translatable block into a tagged string with a side-map for reinsert.</summary>
    public static Fin<ExtractResult> Extract(string editorStateJson)
    {
        try
        {
            var root =
                JsonNode.Parse(editorStateJson) ?? throw new JsonException("Empty editor state");
            return new ExtractResult(editorStateJson, ExtractBlocks(root));
        }
        catch (JsonException ex)
        {
            return Error.New($"Invalid Lexical JSON: {ex.Message}");
        }
    }

    /// <summary>
    /// Rebuilds the editor state from the original, replacing the inline children of each block that
    /// has an entry in <paramref name="translationsByIndex"/>. Blocks without an entry — including
    /// empty spacers and list containers — are left byte-for-byte.
    /// </summary>
    public static Fin<string> Reinsert(
        string originalJson,
        IReadOnlyDictionary<int, string> translationsByIndex
    )
    {
        try
        {
            var root =
                JsonNode.Parse(originalJson) ?? throw new JsonException("Empty editor state");
            foreach (var block in ExtractBlocks(root))
            {
                if (!translationsByIndex.TryGetValue(block.Index, out var translated))
                {
                    continue;
                }
                if (string.IsNullOrWhiteSpace(translated))
                {
                    throw new JsonException($"Block {block.Index} translated to empty");
                }
                LexicalTree.Navigate(root, block.NodePath)["children"] = TagCodec.ParseTagged(
                    translated,
                    block.SideMap
                );
            }
            return root.ToJsonString(RelaxedOptions);
        }
        catch (JsonException ex)
        {
            return Error.New($"Invalid Lexical JSON: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks that a translated block carries the same multiset of tags as its source: same ids,
    /// balanced, none invented, dropped or duplicated.
    /// </summary>
    public static TagValidation ValidateTags(string sourceBlock, string translatedBlock)
    {
        try
        {
            var source = TagCodec.ScanAndCount(sourceBlock);
            var translated = TagCodec.ScanAndCount(translatedBlock);
            if (source.Count != translated.Count)
            {
                return new TagValidation(false, "Tag multiset differs from source");
            }
            foreach (var (key, count) in source)
            {
                if (translated.GetValueOrDefault(key) != count)
                {
                    return new TagValidation(false, "Tag multiset differs from source");
                }
            }
            return new TagValidation(true, null);
        }
        catch (JsonException ex)
        {
            return new TagValidation(false, ex.Message);
        }
    }

    /// <summary>Prepares a plain-string field (title / excerpt) for the same translate path.</summary>
    public static string ExtractPlain(string plain) => TagCodec.Escape(plain);

    /// <summary>Restores a translated plain-string field.</summary>
    public static string ReinsertPlain(string translated) => TagCodec.Unescape(translated);

    /// <summary>
    /// True when two editor states are semantically identical, ignoring object key ordering and the
    /// harmless merging of adjacent same-format text runs. This is the round-trip gate.
    /// </summary>
    public static bool TreesEquivalent(string leftJson, string rightJson)
    {
        var left = JsonNode.Parse(leftJson);
        var right = JsonNode.Parse(rightJson);
        LexicalTree.Canonicalize(left);
        LexicalTree.Canonicalize(right);
        return JsonNode.DeepEquals(left, right);
    }

    static IReadOnlyList<LexicalBlock> ExtractBlocks(JsonNode root)
    {
        var rootChildren =
            root["root"]?["children"]?.AsArray()
            ?? throw new JsonException("Invalid Lexical JSON: missing root.children");
        var blocks = new List<LexicalBlock>();
        for (int i = 0; i < rootChildren.Count; i++)
        {
            Walk(rootChildren[i], [i], blocks);
        }
        return blocks;
    }

    static void Walk(JsonNode? node, int[] path, List<LexicalBlock> blocks)
    {
        if (node is not JsonObject obj)
        {
            return;
        }

        switch (obj["type"]?.GetValue<string>())
        {
            case "list":
                RecurseChildren(obj, path, blocks);
                break;
            case "listitem":
                if (HasListChild(obj))
                {
                    RecurseChildren(obj, path, blocks);
                }
                else
                {
                    EmitBlock(obj, path, BlockType.ListItem, blocks);
                }
                break;
            case "paragraph":
                EmitBlock(obj, path, BlockType.Paragraph, blocks);
                break;
            case "heading":
                EmitBlock(obj, path, BlockType.Heading, blocks);
                break;
            default:
                break;
        }
    }

    static void RecurseChildren(JsonObject obj, int[] path, List<LexicalBlock> blocks)
    {
        if (obj["children"]?.AsArray() is not { } children)
        {
            return;
        }
        for (int j = 0; j < children.Count; j++)
        {
            Walk(children[j], [.. path, j], blocks);
        }
    }

    static bool HasListChild(JsonObject listItem)
    {
        if (listItem["children"]?.AsArray() is not { } children)
        {
            return false;
        }
        foreach (var child in children)
        {
            if (child is JsonObject obj && obj["type"]?.GetValue<string>() == "list")
            {
                return true;
            }
        }
        return false;
    }

    static void EmitBlock(JsonObject block, int[] path, BlockType type, List<LexicalBlock> blocks)
    {
        var (source, sideMap, visible) = SerializeInline(block["children"]?.AsArray());
        if (string.IsNullOrWhiteSpace(visible))
        {
            return;
        }
        blocks.Add(new LexicalBlock(blocks.Count, type, path, source, sideMap));
    }

    static (
        string Source,
        IReadOnlyDictionary<int, InlineTag> SideMap,
        string Visible
    ) SerializeInline(JsonArray? children)
    {
        var source = new StringBuilder();
        var visible = new StringBuilder();
        var sideMap = new Dictionary<int, InlineTag>();
        int id = 0;

        if (children is not null)
        {
            foreach (var child in children)
            {
                if (child is not JsonObject node)
                {
                    continue;
                }

                switch (node["type"]?.GetValue<string>())
                {
                    case "text":
                    {
                        int format = node["format"]?.GetValue<int>() ?? 0;
                        int detail = node["detail"]?.GetValue<int>() ?? 0;
                        string mode = node["mode"]?.GetValue<string>() ?? "normal";
                        string style = node["style"]?.GetValue<string>() ?? "";
                        string text = node["text"]?.GetValue<string>() ?? "";

                        if (format == 0 && detail == 0 && mode == "normal" && style.Length == 0)
                        {
                            source.Append(TagCodec.Escape(text));
                        }
                        else
                        {
                            int k = id++;
                            sideMap[k] = new InlineTag(
                                k,
                                InlineKind.FormatRun,
                                format,
                                detail,
                                mode,
                                style
                            );
                            AppendRun(source, k, text);
                        }
                        visible.Append(text);
                        break;
                    }

                    case "link" when IsSimpleLink(node):
                    {
                        int k = id++;
                        var anchor = node["children"]!.AsArray()[0]!.AsObject();
                        var template = node.DeepClone();
                        template.AsObject().Remove("children");
                        sideMap[k] = new InlineTag(
                            k,
                            InlineKind.Link,
                            anchor["format"]?.GetValue<int>() ?? 0,
                            anchor["detail"]?.GetValue<int>() ?? 0,
                            anchor["mode"]?.GetValue<string>() ?? "normal",
                            anchor["style"]?.GetValue<string>() ?? "",
                            ReadLinkAttrs(node),
                            template
                        );
                        string anchorText = anchor["text"]?.GetValue<string>() ?? "";
                        AppendRun(source, k, anchorText);
                        visible.Append(anchorText);
                        break;
                    }

                    case "linebreak":
                    {
                        int k = id++;
                        sideMap[k] = new InlineTag(k, InlineKind.LineBreak);
                        source.Append("<br").Append(k).Append("/>");
                        break;
                    }

                    default:
                    {
                        int k = id++;
                        sideMap[k] = new InlineTag(
                            k,
                            InlineKind.Opaque,
                            NodeTemplate: node.DeepClone()
                        );
                        source.Append('<').Append(k).Append("/>");
                        break;
                    }
                }
            }
        }

        return (source.ToString(), sideMap, visible.ToString());
    }

    static void AppendRun(StringBuilder source, int id, string text) =>
        source
            .Append('<')
            .Append(id)
            .Append('>')
            .Append(TagCodec.Escape(text))
            .Append("</")
            .Append(id)
            .Append('>');

    static bool IsSimpleLink(JsonObject link) =>
        link["children"]?.AsArray() is { Count: 1 } children
        && children[0] is JsonObject child
        && child["type"]?.GetValue<string>() == "text";

    static LinkAttrs ReadLinkAttrs(JsonObject link) =>
        new(
            link["url"]?.GetValue<string>(),
            link["rel"]?.GetValue<string>(),
            link["target"]?.GetValue<string>(),
            link["title"]?.GetValue<string>()
        );
}
