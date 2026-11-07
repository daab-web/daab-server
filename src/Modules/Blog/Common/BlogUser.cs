using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Daab.Modules.Blog.Common;

/// <summary>The signed-in user as the blog sees them, read from the access-token claims.</summary>
public sealed record BlogUser(string Id, string Name, string? ScientistId, bool IsAdmin)
{
    // Claim names mirror the Auth module's token; Blog does not reference Auth.
    public const string Scheme = "Bearer";
    public const string ScientistRole = "scientist";
    public const string AdminRole = "admin";

    public static BlogUser? From(ClaimsPrincipal principal)
    {
        var id = principal.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        var roles = principal
            .FindAll("roles")
            .SelectMany(c => c.Value.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .ToHashSet(StringComparer.Ordinal);

        return new BlogUser(
            id,
            principal.FindFirstValue("name") ?? "Anonymous",
            principal.FindFirstValue("scientist_id"),
            roles.Contains(AdminRole)
        );
    }

    /// <summary>
    /// Authenticates the bearer token on an anonymous endpoint, where the endpoint itself does
    /// not require it. Returns <c>null</c> for anonymous callers.
    /// </summary>
    public static async Task<BlogUser?> FromOptionalAuthAsync(HttpContext httpContext)
    {
        var result = await httpContext.AuthenticateAsync(Scheme);
        return result.Succeeded ? From(result.Principal) : null;
    }
}
