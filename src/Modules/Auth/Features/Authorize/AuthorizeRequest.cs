using FastEndpoints;

namespace Daab.Modules.Auth.Features.Authorize;

public sealed record AuthorizeRequest
{
    [QueryParam, BindFrom("response_type")]
    public required string ResponseType { get; init; }

    [QueryParam, BindFrom("client_id")]
    public required string ClientId { get; init; }

    [QueryParam, BindFrom("code_challenge")]
    public required string CodeChallenge { get; init; }

    [QueryParam, BindFrom("code_challenge_method")]
    public required string CodeChallengeMethod { get; init; }

    [QueryParam, BindFrom("redirect_uri")]
    public required string RedirectUri { get; init; }

    [QueryParam, BindFrom("scope")]
    public required string Scope { get; init; }

    [QueryParam, BindFrom("state")]
    public required string State { get; init; }
}
