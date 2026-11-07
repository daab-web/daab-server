using System.Text.Json.Serialization;

namespace Daab.Modules.Blog.Models;

[JsonConverter(typeof(JsonStringEnumConverter<PostStatus>))]
public enum PostStatus
{
    /// <summary>Being written or edited by the author; not visible to readers.</summary>
    Draft,

    /// <summary>Passed content verification and is publicly visible.</summary>
    Published,

    /// <summary>Failed content verification; see <see cref="Post.VerificationFeedback"/>.</summary>
    Rejected,
}
