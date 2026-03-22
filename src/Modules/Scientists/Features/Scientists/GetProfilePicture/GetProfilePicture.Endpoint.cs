using Daab.Modules.Scientists.Persistence;
using Daab.SharedKernel.Extensions;
using FastEndpoints;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Scientists.Features.Scientists.GetProfilePicture;

public sealed record GetProfilePictureResponse(string ScientistId, string? ImageUrl);

public class GetProfilePictureEndpoint(IMediator mediator)
    : EndpointWithoutRequest<GetProfilePictureResponse>
{
    public override void Configure()
    {
        Get("/scientists/{scientistId}/profile-picture");

        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var scientistId = Route<string>("scientistId");
        if (string.IsNullOrWhiteSpace(scientistId))
        {
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        var res = await mediator.Send(new GetProfilePictureQuery(scientistId), ct);

        await res.Match(
            url => Send.OkAsync(new GetProfilePictureResponse(scientistId, url), cancellation: ct),
            err => err.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
        );
    }
}

public sealed class GetProfilePictureQuery(string scientistId) : IRequest<Fin<string?>>
{
    public string ScientistId { get; } = scientistId;
}

public sealed class GetProfilePictureQueryHandler(ScientistsDbContext ctx)
    : IRequestHandler<GetProfilePictureQuery, Fin<string?>>
{
    public async Task<Fin<string?>> Handle(
        GetProfilePictureQuery request,
        CancellationToken cancellationToken
    )
    {
        var res = await ctx
            .Scientists.SingleOrDefaultAsync(s => s.Id == request.ScientistId, cancellationToken)
            .Select(s => new { s?.PhotoUrl });

        return res switch
        {
            null => Error.New(StatusCodes.Status404NotFound, "Scientist not found"),
            _ => res.PhotoUrl,
        };
    }
}
