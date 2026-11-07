using Daab.Modules.Auth.Common;
using Daab.SharedKernel.Extensions;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Daab.Modules.Auth.Features.Users.SetUserRole;

/// <summary>
/// <c>PUT /users/{id}/roles/{role}</c> grants a role, <c>DELETE</c> removes it. The change shows
/// up in the user's next access token (at their next sign-in).
/// </summary>
public sealed class GrantUserRoleEndpoint(IMediator mediator)
    : SetUserRoleEndpointBase(mediator, granted: true)
{
    public override void Configure()
    {
        Put("/users/{id}/roles/{role}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Roles(AuthRoles.Admin);
    }
}

public sealed class RevokeUserRoleEndpoint(IMediator mediator)
    : SetUserRoleEndpointBase(mediator, granted: false)
{
    public override void Configure()
    {
        Delete("/users/{id}/roles/{role}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Roles(AuthRoles.Admin);
    }
}

public abstract class SetUserRoleEndpointBase(IMediator mediator, bool granted)
    : EndpointWithoutRequest<SetUserRoleResponse>
{
    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await mediator.Send(
            new SetUserRoleCommand(Route<string>("id")!, Route<string>("role")!, granted),
            ct
        );

        await result.Match(
            roles => Send.OkAsync(roles, ct),
            err => err.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
        );
    }
}
