namespace Daab.Modules.Auth.Features.Users.GetUsers;

/// <param name="HasPassword">False for Google-only accounts and invitations not yet accepted.</param>
/// <param name="Providers">External sign-in providers linked to the account, e.g. <c>google</c>.</param>
public sealed record UserSummaryResponse(
    string Id,
    string Username,
    string? Email,
    string Name,
    string? PictureUrl,
    IReadOnlyList<string> Roles,
    bool HasPassword,
    IReadOnlyList<string> Providers,
    DateTime CreatedAt
);
