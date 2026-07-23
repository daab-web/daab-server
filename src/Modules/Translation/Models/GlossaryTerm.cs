namespace Daab.Modules.Translation.Models;

/// <summary>
/// A single forced-terminology pair: whenever <see cref="Source"/> appears in the
/// source text it must be rendered as <see cref="Target"/> in the translation.
/// <see cref="Namespace"/> carries provenance from the ReferenceData namespace the
/// term was projected from (e.g. institutions, areas).
/// </summary>
public sealed record GlossaryTerm(string Namespace, string Source, string Target);
