using Daab.Modules.Scientists.Features.GetAllApplications;
using Daab.SharedKernel.Extensions;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Daab.Modules.Scientists.Features.Applications.GetApplication;

public sealed class GetApplicationEndpoint(IMediator mediator)
    : EndpointWithoutRequest<ApplicationDto>
{
    public override void Configure()
    {
        Get("/applications/{applicationId}");

        // TODO: This should be allowed only for admin
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<string>("applicationId");

        if (string.IsNullOrWhiteSpace(id))
        {
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        var result = await mediator.Send(new GetApplicationQuery(id), ct);

        await result.Match(
            application => Send.OkAsync(application, ct),
            err => Send.ResultAsync(TypedResults.Problem(err.ToProblemDetails(HttpContext)))
        );
    }
}
