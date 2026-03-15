using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Daab.Modules.Auth.Persistence;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Daab.Modules.Auth.Features.UserInfo;

public sealed record UserInfoResponse(string Sub, string Email, string Name);

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

        await Send.OkAsync(
            new UserInfoResponse(user.Id, "admin@waas.com", user.Username),
            cancellation: ct
        );
    }
}
