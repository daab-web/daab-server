using Daab.Modules.ReferenceData.Database;
using Daab.Modules.Translation.Glossary;
using Microsoft.EntityFrameworkCore;

namespace Daab.Web.Translation;

/// <summary>
/// Host-side bridge that feeds ReferenceData translation rows into the module's glossary
/// projection. It lives here, not in the translation module, because the modules are isolated —
/// only the web host references both.
/// </summary>
internal sealed class ReferenceDataGlossaryProvider(ReferenceDataDbContext context)
    : IGlossaryProvider
{
    public async Task<Glossary> GetGlossaryAsync(
        string sourceLocale,
        string targetLocale,
        CancellationToken cancellationToken
    )
    {
        var rows = await context
            .Translations.Where(t => t.Locale == sourceLocale || t.Locale == targetLocale)
            .Select(t => new GlossaryRow(t.Namespace, t.Key, t.Locale, t.Value))
            .ToListAsync(cancellationToken);

        return GlossaryProjection.Project(rows, sourceLocale, targetLocale);
    }
}
