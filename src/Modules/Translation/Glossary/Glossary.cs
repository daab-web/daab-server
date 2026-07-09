using Daab.Modules.Translation.Models;

namespace Daab.Modules.Translation.Glossary;

/// <summary>
/// The projected set of forced-terminology pairs for one source→target locale
/// direction. Ordering is deterministic so the rendered block is byte-stable
/// across calls — it lives in the Gemini context-cache prefix (Step 4), which
/// only pays off if the prefix is identical every request.
/// </summary>
public sealed record Glossary(IReadOnlyList<GlossaryTerm> Terms)
{
    public static Glossary Empty { get; } = new([]);

    public bool IsEmpty => Terms.Count == 0;

    /// <summary>
    /// Renders one <c>"source" → "target"</c> line per term. The instruction
    /// wrapper ("always render exactly as given") is prepended at prompt
    /// assembly time (Step 4); this keeps the term list composable.
    /// </summary>
    public string Render() =>
        string.Join('\n', Terms.Select(t => $"\"{t.Source}\" → \"{t.Target}\""));
}
