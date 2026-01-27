using Daab.Modules.Scientists.Persistence;
using FastEndpoints;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Scientists.Features.Applications.Approve;

public sealed record ApproveApplicationResponse(string ScientistId);

public sealed class ApproveApplicetionEndpoint(IMediator mediator) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("/applications/{applicationId}/approve");

        // TODO: this should be available for admins only
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var applicationId = Route<string>("applicationId");

        if (applicationId is null)
        {
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        var result = await mediator.Send(new ApproveApplicationCommand(applicationId), ct);
    }
}

public sealed record ApproveApplicationCommand : IRequest<Fin<ApproveApplicationResponse>>
{
    public string ApplicationId { get; }

    public ApproveApplicationCommand(string applicationId)
    {
        ApplicationId = applicationId;
    }
}

public sealed class ApproveApplicationCommandHandler(ScientistsDbContext context)
    : IRequestHandler<ApproveApplicationCommand, Fin<ApproveApplicationResponse>>
{
    async Task<Fin<ApproveApplicationResponse>> IRequestHandler<
        ApproveApplicationCommand,
        Fin<ApproveApplicationResponse>
    >.Handle(ApproveApplicationCommand request, CancellationToken cancellationToken)
    {
        var application = await context.Applications.SingleOrDefaultAsync(
            a => a.Id == request.ApplicationId,
            cancellationToken: cancellationToken
        );

        if (application is null)
        {
            return Error.New("Requested application does not exist");
        }

        return new ApproveApplicationResponse(Ulid.NewUlid().ToString());
    }
}
