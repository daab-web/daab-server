using Daab.Modules.Auth.Common;
using Daab.SharedKernel;
using Daab.SharedKernel.Extensions;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Daab.Modules.Auth.Features.Users.GetUsers;

public sealed class GetUsersEndpoint(IMediator mediator)
    : Endpoint<GetUsersRequest, PagedResponse<UserSummaryResponse>>
{
    public override void Configure()
    {
        Get("/users");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Roles(AuthRoles.Admin);
    }

    public override async Task HandleAsync(GetUsersRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new GetUsersQuery(req.Search, req.Page, req.PageSize), ct);

        await result.Match(
            users => Send.OkAsync(users, ct),
            err => err.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
        );
    }
}
