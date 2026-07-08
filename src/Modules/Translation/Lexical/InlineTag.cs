using System.Text.Json.Nodes;

namespace Daab.Modules.Translation.Lexical;

/// <summary>Kind of inline element a per-block tag stands in for.</summary>
public enum InlineKind
{
    /// <summary>A text run carrying a non-default <c>format</c> bitmask (bold, italic, …).</summary>
    FormatRun,

    /// <summary>A link node; its anchor text is translatable, its attributes are not.</summary>
    Link,

    /// <summary>A <c>linebreak</c> node.</summary>
    LineBreak,

    /// <summary>Any inline node we do not translate; preserved verbatim from a stored clone.</summary>
    Opaque,
}

/// <summary>The non-translatable attributes of a Lexical link node.</summary>
public sealed record LinkAttrs(string? Url, string? Rel, string? Target, string? Title);

/// <summary>
/// One entry in a block's side-map, keyed by the tag id used in the serialized block string.
/// Carries whatever is needed to rebuild the inline node during reinsert without ever asking the
/// translator to preserve it.
/// </summary>
public sealed record InlineTag(
    int Id,
    InlineKind Kind,
    int Format = 0,
    int Detail = 0,
    string Mode = "normal",
    string Style = "",
    LinkAttrs? Link = null,
    JsonNode? NodeTemplate = null
);
