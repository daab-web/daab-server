namespace Daab.Modules.Translation.Lexical;

/// <summary>The kind of block a translatable string was extracted from.</summary>
public enum BlockType
{
    Paragraph,
    Heading,
    ListItem,
}

/// <summary>
/// A single translatable unit: one block's inline children flattened into a tagged string, plus
/// everything needed to splice a translation back into the exact same spot in the tree.
/// </summary>
/// <param name="Index">0-based position in document order across all emitted blocks.</param>
/// <param name="NodePath">Child indices from <c>root.children</c> down to this block's node.</param>
/// <param name="Source">The tagged, escaped inline string for the block.</param>
/// <param name="SideMap">Tag id → the inline node it stands in for.</param>
public sealed record LexicalBlock(
    int Index,
    BlockType BlockType,
    IReadOnlyList<int> NodePath,
    string Source,
    IReadOnlyDictionary<int, InlineTag> SideMap
);
