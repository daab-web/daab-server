using System.Security.Claims;
using FastEndpoints;

namespace Daab.Modules.Auth.Features.VerifyAdmin;

public class VerifyAdminEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/admin");
        Roles("admin");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await Send.OkAsync(cancellation: ct);
    }
}
