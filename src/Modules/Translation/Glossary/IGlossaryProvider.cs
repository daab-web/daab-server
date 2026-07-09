namespace Daab.Modules.Translation.Glossary;

/// <summary>
/// Seam over the glossary source. The Step-4 host-side implementation reads
/// <c>ReferenceData.Translation</c> rows and hands them to
/// <see cref="GlossaryProjection.Project"/>; the module itself takes no
/// dependency on the ReferenceData module (modules are isolated, wired only at
/// the Web host — mirrors the <c>IEmbedder</c> seam).
/// </summary>
public interface IGlossaryProvider
{
    Task<Glossary> GetGlossaryAsync(
        string sourceLocale,
        string targetLocale,
        CancellationToken cancellationToken
    );
}
