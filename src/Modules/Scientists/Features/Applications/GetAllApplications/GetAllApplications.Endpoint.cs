using Daab.SharedKernel;
using Daab.SharedKernel.Extensions;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Daab.Modules.Scientists.Features.GetAllApplications;

public sealed class GetAllApplicationsEndpoint(IMediator mediator)
    : Endpoint<GetAllApplicationsRequest, PagedResponse<ApplicationDto>>
{
    public override void Configure()
    {
        Get("/applications");

        // TODO: This should be allowed only for admin
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetAllApplicationsRequest req, CancellationToken ct)
    {
        // TODO: request validation

        var result = await mediator.Send(new GetAllApplicationsQuery(req), ct);

        await result.Match(
            Send.OkAsync,
            err => Send.ResultAsync(TypedResults.Problem(err.ToProblemDetails(HttpContext)))
        );
    }
}
