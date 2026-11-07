using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Daab.Modules.Auth.Common;
using Daab.Modules.Auth.Persistence;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Daab.Modules.Auth.Features.UserInfo;

public sealed record UserInfoResponse(
    string Sub,
    string? Email,
    string Name,
    IReadOnlyList<string> Roles,
    string? ScientistId,
    string? PictureUrl
);

public class UserInfo(AuthDbContext ctx) : EndpointWithoutRequest<UserInfoResponse>
{
    public override void Configure()
    {
        Get("/userinfo");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (userId is null)
        {
            await Send.UnauthorizedAsync(cancellation: ct);
            return;
        }

        var user = await ctx.Users.FindAsync([userId], cancellationToken: ct);

        if (user is null)
        {
            await Send.NotFoundAsync(cancellation: ct);
            return;
        }

        // Roles come from the token: some (e.g. scientist) are granted by other modules at
        // token issuance and are not stored in the Auth database.
        var roles = User.FindAll(AuthClaims.Roles)
            .SelectMany(c => c.Value.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        await Send.OkAsync(
            new UserInfoResponse(
                user.Id,
                user.Email,
                user.DisplayName,
                roles,
                User.FindFirstValue(AuthClaims.ScientistId),
                user.PictureUrl
            ),
            cancellation: ct
        );
    }
}
