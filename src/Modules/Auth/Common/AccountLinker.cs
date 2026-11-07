namespace Daab.Modules.Auth.Common;

/// <summary>
/// Roles and claims a user gets from outside the Auth module (e.g. being a scientist).
/// </summary>
public sealed record AccountLink(
    IReadOnlyList<string> Roles,
    IReadOnlyDictionary<string, string> Claims
)
{
    public static AccountLink None { get; } = new([], new Dictionary<string, string>());
}

/// <summary>
/// Seam the host implements to attach identities owned by other modules to an Auth user.
/// The Auth module never references those modules; see <c>Daab.Web/Accounts</c>.
/// </summary>
public interface IAccountLinker
{
    Task<AccountLink> LinkAsync(string userId, string? email, CancellationToken cancellationToken);
}

internal sealed class NullAccountLinker : IAccountLinker
{
    public Task<AccountLink> LinkAsync(
        string userId,
        string? email,
        CancellationToken cancellationToken
    ) => Task.FromResult(AccountLink.None);
}

public static class AuthClaims
{
    public const string Roles = "roles";
    public const string ScientistId = "scientist_id";
}

public static class AuthRoles
{
    public const string Admin = "admin";
    public const string User = "user";
    public const string Scientist = "scientist";

    /// <summary>Roles an administrator may grant or remove from the Users panel.</summary>
    public static readonly IReadOnlySet<string> Assignable = new HashSet<string>(
        StringComparer.Ordinal
    )
    {
        Scientist,
    };
}
