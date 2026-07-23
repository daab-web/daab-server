using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Daab.Modules.Translation.Lexical;

internal enum TagKind
{
    Open,
    Close,
    LineBreak,
    Opaque,
}

internal enum TagCategory
{
    Pair,
    LineBreak,
    Opaque,
}

internal readonly record struct TagToken(TagKind Kind, int Id, int End);

/// <summary>
/// Turns inline runs into a compact tagged string and back. The escaping scheme guarantees that
/// the only unescaped <c>&lt;</c> characters in a block string are tag delimiters, so literal
/// angle brackets in the source text can never be mistaken for markup.
/// </summary>
internal static class TagCodec
{
    public static string Escape(string text) =>
        text.Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal);

    public static string Unescape(string text) =>
        text.Replace("&lt;", "<", StringComparison.Ordinal)
            .Replace("&gt;", ">", StringComparison.Ordinal)
            .Replace("&amp;", "&", StringComparison.Ordinal);

    /// <summary>Matches the tag starting at <paramref name="i"/> (which must be a <c>&lt;</c>).</summary>
    public static TagToken MatchTag(string s, int i)
    {
        int n = s.Length;
        int p = i + 1;
        if (p >= n)
        {
            throw new JsonException($"Malformed tag at position {i}");
        }

        if (s[p] == '/')
        {
            p++;
            int start = p;
            while (p < n && char.IsAsciiDigit(s[p]))
            {
                p++;
            }
            if (p == start || p >= n || s[p] != '>')
            {
                throw new JsonException($"Malformed closing tag at position {i}");
            }
            return new TagToken(TagKind.Close, ParseId(s, start, p), p + 1);
        }

        if (s[p] == 'b' && p + 1 < n && s[p + 1] == 'r')
        {
            p += 2;
            int start = p;
            while (p < n && char.IsAsciiDigit(s[p]))
            {
                p++;
            }
            if (p == start || p + 1 >= n || s[p] != '/' || s[p + 1] != '>')
            {
                throw new JsonException($"Malformed linebreak tag at position {i}");
            }
            return new TagToken(TagKind.LineBreak, ParseId(s, start, p), p + 2);
        }

        if (char.IsAsciiDigit(s[p]))
        {
            int start = p;
            while (p < n && char.IsAsciiDigit(s[p]))
            {
                p++;
            }
            if (p < n && s[p] == '>')
            {
                return new TagToken(TagKind.Open, ParseId(s, start, p), p + 1);
            }
            if (p + 1 < n && s[p] == '/' && s[p + 1] == '>')
            {
                return new TagToken(TagKind.Opaque, ParseId(s, start, p), p + 2);
            }
        }

        throw new JsonException($"Malformed tag at position {i}");
    }

    static int ParseId(string s, int start, int end)
    {
        if (!int.TryParse(s.AsSpan(start, end - start), out int id))
        {
            throw new JsonException($"Tag id out of range at position {start}");
        }
        return id;
    }

    /// <summary>Parses a tagged (and escaped) block string back into Lexical inline nodes.</summary>
    public static JsonArray ParseTagged(string s, IReadOnlyDictionary<int, InlineTag> sideMap)
    {
        var nodes = new JsonArray();
        var buffer = new StringBuilder();
        int i = 0;
        int n = s.Length;

        while (i < n)
        {
            char c = s[i];
            if (c != '<')
            {
                buffer.Append(c);
                i++;
                continue;
            }

            FlushText(nodes, buffer);
            var token = MatchTag(s, i);
            switch (token.Kind)
            {
                case TagKind.Close:
                    throw new JsonException($"Unexpected closing tag at position {i}");

                case TagKind.LineBreak:
                    _ = RequireKind(sideMap, token.Id, InlineKind.LineBreak);
                    nodes.Add(LexicalTree.MakeLineBreak());
                    i = token.End;
                    break;

                case TagKind.Opaque:
                {
                    var tag = RequireKind(sideMap, token.Id, InlineKind.Opaque);
                    nodes.Add(TemplateClone(tag));
                    i = token.End;
                    break;
                }

                case TagKind.Open:
                {
                    if (!sideMap.TryGetValue(token.Id, out var tag))
                    {
                        throw new JsonException($"Invented tag id {token.Id}");
                    }
                    if (tag.Kind is not (InlineKind.FormatRun or InlineKind.Link))
                    {
                        throw new JsonException($"Tag id {token.Id} is not an inline run");
                    }

                    int innerStart = token.End;
                    int close = s.IndexOf('<', innerStart);
                    if (close < 0)
                    {
                        throw new JsonException($"Unclosed tag {token.Id}");
                    }
                    var closeToken = MatchTag(s, close);
                    if (closeToken.Kind != TagKind.Close || closeToken.Id != token.Id)
                    {
                        throw new JsonException($"Unbalanced tag {token.Id}");
                    }

                    string inner = Unescape(s.Substring(innerStart, close - innerStart));
                    nodes.Add(BuildRun(tag, inner));
                    i = closeToken.End;
                    break;
                }

                default:
                    throw new JsonException($"Unknown tag at position {i}");
            }
        }

        FlushText(nodes, buffer);
        return nodes;
    }

    static JsonNode BuildRun(InlineTag tag, string inner)
    {
        var text = LexicalTree.MakeText(tag.Format, tag.Detail, tag.Mode, tag.Style, inner);
        if (tag.Kind != InlineKind.Link)
        {
            return text;
        }

        var link = TemplateClone(tag);
        link["children"] = new JsonArray(text);
        return link;
    }

    static JsonNode TemplateClone(InlineTag tag) =>
        (
            tag.NodeTemplate ?? throw new JsonException($"Missing template for tag {tag.Id}")
        ).DeepClone();

    static void FlushText(JsonArray nodes, StringBuilder buffer)
    {
        if (buffer.Length == 0)
        {
            return;
        }
        nodes.Add(LexicalTree.MakeText(0, 0, "normal", "", Unescape(buffer.ToString())));
        buffer.Clear();
    }

    static InlineTag RequireKind(
        IReadOnlyDictionary<int, InlineTag> sideMap,
        int id,
        InlineKind kind
    )
    {
        if (!sideMap.TryGetValue(id, out var tag) || tag.Kind != kind)
        {
            throw new JsonException($"Invented or mismatched tag id {id}");
        }
        return tag;
    }

    /// <summary>
    /// Structurally validates a tagged string (balanced, no overlap, no orphan close) and returns
    /// the multiset of tags it contains. Throws <see cref="JsonException"/> on malformed input.
    /// </summary>
    public static Dictionary<(int Id, TagCategory Category), int> ScanAndCount(string s)
    {
        var counts = new Dictionary<(int Id, TagCategory Category), int>();
        var open = new Stack<int>();
        int i = 0;
        int n = s.Length;

        while (i < n)
        {
            if (s[i] != '<')
            {
                i++;
                continue;
            }

            var token = MatchTag(s, i);
            switch (token.Kind)
            {
                case TagKind.Open:
                    open.Push(token.Id);
                    break;
                case TagKind.Close:
                    if (open.Count == 0 || open.Peek() != token.Id)
                    {
                        throw new JsonException($"Unbalanced closing tag {token.Id}");
                    }
                    open.Pop();
                    Increment(counts, (token.Id, TagCategory.Pair));
                    break;
                case TagKind.LineBreak:
                    Increment(counts, (token.Id, TagCategory.LineBreak));
                    break;
                case TagKind.Opaque:
                    Increment(counts, (token.Id, TagCategory.Opaque));
                    break;
                default:
                    throw new JsonException($"Unknown tag at position {i}");
            }
            i = token.End;
        }

        if (open.Count > 0)
        {
            throw new JsonException("Unclosed tag in block");
        }
        return counts;
    }

    static void Increment(
        Dictionary<(int Id, TagCategory Category), int> counts,
        (int Id, TagCategory Category) key
    ) => counts[key] = counts.GetValueOrDefault(key) + 1;
}
