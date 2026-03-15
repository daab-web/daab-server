using System.Text.Json.Serialization;
using FastEndpoints;

namespace Daab.Modules.Auth.Features.Token;

public class OAuthTokenRequest
{
    [BindFrom("grant_type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required string GrantType { get; init; }

    [BindFrom("code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required string Code { get; init; } = string.Empty;

    [BindFrom("redirect_uri")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required string RedirectUri { get; init; }

    [BindFrom("code_verifier")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required string CodeVerifier { get; init; }
}
