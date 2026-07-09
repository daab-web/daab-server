using Daab.Modules.Translation.Glossary;
using GlossarySet = Daab.Modules.Translation.Glossary.Glossary;

namespace Daab.Modules.Translation.Tests.Service;

internal sealed class FakeGlossaryProvider(GlossarySet glossary) : IGlossaryProvider
{
    public static FakeGlossaryProvider Empty { get; } = new(GlossarySet.Empty);

    public Task<GlossarySet> GetGlossaryAsync(
        string sourceLocale,
        string targetLocale,
        CancellationToken cancellationToken
    ) => Task.FromResult(glossary);
}
