using System.Security.Claims;
using FastEndpoints;

namespace Daab.Modules.Auth.Features.VerifyAdmin;

public class VerifyAdminEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/admin");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var roleClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);

        if (roleClaim is not { Value: "admin" })
        {
            await Send.ForbiddenAsync(ct);
            return;
        }

        await Send.OkAsync(cancellation: ct);
    }
}