using LanguageExt;

namespace Daab.Modules.Blog.Common;

/// <summary>Plain-text view of a post handed to the verifier.</summary>
public sealed record ContentVerificationRequest(
    string Title,
    string Excerpt,
    string Body,
    string Locale
);

/// <param name="Approved">Whether the post may be published.</param>
/// <param name="Reasons">Why it was rejected; empty when approved.</param>
public sealed record ContentVerificationResult(bool Approved, IReadOnlyList<string> Reasons)
{
    public static ContentVerificationResult Approve() => new(true, []);

    public static ContentVerificationResult Reject(params string[] reasons) => new(false, reasons);
}

/// <summary>
/// Decides whether a post's content may be published (moderation). The host implements it — the
/// Blog module does not reference the module that talks to the AI model; see
/// <c>Daab.Web/Blog</c>. A failed <see cref="Fin{A}"/> means verification could not run, not that
/// the post was rejected.
/// </summary>
public interface IContentVerifier
{
    Task<Fin<ContentVerificationResult>> VerifyAsync(
        ContentVerificationRequest request,
        CancellationToken cancellationToken
    );
}

/// <summary>Cheap deterministic checks run before the verifier is called.</summary>
public static class ContentRules
{
    public const int MinBodyLength = 100;

    public static ContentVerificationResult Check(string title, string body)
    {
        var reasons = new List<string>();

        if (title.Trim().Length < 5)
        {
            reasons.Add("The title is too short.");
        }

        if (body.Length < MinBodyLength)
        {
            reasons.Add($"The article must contain at least {MinBodyLength} characters of text.");
        }

        return reasons.Count == 0
            ? ContentVerificationResult.Approve()
            : ContentVerificationResult.Reject([.. reasons]);
    }
}
