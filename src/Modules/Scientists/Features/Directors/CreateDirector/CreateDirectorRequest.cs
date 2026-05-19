using Daab.SharedKernel.Middlewares;

namespace Daab.Modules.Scientists.Features.Directors.CreateDirector;

public sealed record DirectorTranslationEntry(string Locale, string Role);

public sealed record CreateDirectorRequest(
    string ScientistId,
    DirectorTranslationEntry[] Translations
) : ILocalizedCollection
{
    public IEnumerable<string> GetLocales() => Translations.Select(t => t.Locale);
}
