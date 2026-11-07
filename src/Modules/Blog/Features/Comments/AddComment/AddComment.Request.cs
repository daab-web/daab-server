using Daab.SharedKernel.Middlewares;

namespace Daab.Modules.Blog.Features.Comments.AddComment;

/// <param name="SourceLocale">The language the commenter wrote in.</param>
/// <param name="Texts">The comment in every supported language, keyed by locale.</param>
public sealed record AddCommentRequest(
    string PostId,
    string SourceLocale,
    Dictionary<string, string> Texts
) : ILocalizedCollection
{
    public IEnumerable<string> GetLocales() => Texts.Keys.Append(SourceLocale);
}
