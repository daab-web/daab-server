using Daab.Modules.Translation.Models;

namespace Daab.Modules.Translation.Glossary;

/// <summary>
/// Projects ReferenceData translation rows into a <see cref="Glossary"/> of
/// forced source→target term pairs. Pure and side-effect free: group rows by
/// (namespace, key), pair the source-locale value with the target-locale value,
/// and drop pairs that carry no forcing signal (missing side, identical text).
/// </summary>
public static class GlossaryProjection
{
    public static Glossary Project(
        IEnumerable<GlossaryRow> rows,
        string sourceLocale,
        string targetLocale
    )
    {
        var terms = new List<GlossaryTerm>();
        var seen = new HashSet<(string Source, string Target)>();

        foreach (var group in rows.GroupBy(r => (r.Namespace, r.Key)))
        {
            var source = ValueFor(group, sourceLocale);
            var target = ValueFor(group, targetLocale);

            // Need both sides to force a rendering; identical text needs no rule.
            if (source is null || target is null)
            {
                continue;
            }

            if (string.Equals(source, target, StringComparison.Ordinal))
            {
                continue;
            }

            if (seen.Add((source, target)))
            {
                terms.Add(new GlossaryTerm(group.Key.Namespace, source, target));
            }
        }

        // Ordinal order keeps the rendered prefix byte-stable for context caching.
        var ordered = terms
            .OrderBy(t => t.Source, StringComparer.Ordinal)
            .ThenBy(t => t.Target, StringComparer.Ordinal)
            .ThenBy(t => t.Namespace, StringComparer.Ordinal)
            .ToList();

        return new Glossary(ordered);
    }

    static string? ValueFor(IEnumerable<GlossaryRow> group, string locale)
    {
        var value = group
            .FirstOrDefault(r => string.Equals(r.Locale, locale, StringComparison.Ordinal))
            ?.Value;

        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
