using System.Threading.Channels;
using Daab.Modules.Scientists.Messages;
using Daab.Modules.Scientists.Persistence;
using Daab.SharedKernel.Constants;
using Daab.SharedKernel.Extensions;
using FastEndpoints;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Daab.Modules.Scientists.Features.Scientists.UpdateProfilePicture;

public class UpdateProfilePictureEndpoint(IMediator mediator) : EndpointWithoutRequest
{
    private static readonly LanguageExt.HashSet<string> AllowedType =
    [
        "image/jpeg",
        "image/png",
        "image/webp",
    ];

    public override void Configure()
    {
        Put("/scientists/{scientistId}/profile-picture");
        AllowFileUploads();

        // TODO: This should not be public
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

        if (Files.Count > 0)
        {
            var file = Files[0];

            if (!AllowedType.Contains(file.ContentType))
            {
                await Send.ErrorsAsync(cancellation: ct);
                return;
            }

            var res = await mediator.Send(new UpdateProfilePictureCommand(scientistId, file), ct);

            await res.Match(
                _ =>
                    Send.AcceptedAtAsync<UpdateProfilePictureEndpoint>(
                        routeValues: scientistId,
                        verb: Http.PUT,
                        generateAbsoluteUrl: true,
                        cancellation: ct
                    ),
                err => err.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
            );

            return;
        }

        await Send.ErrorsAsync(cancellation: ct);
    }
}

public sealed class UpdateProfilePictureCommand(string scientistId, IFormFile image)
    : IRequest<Fin<LanguageExt.Unit>>
{
    public string ScientistId { get; } = scientistId;
    public IFormFile Image { get; } = image;
}

public sealed class UpdateProfilePictureCommandHandler(
    ScientistsDbContext ctx,
    [FromKeyedServices(ChannelKeys.ProfilePictureUpload)]
        Channel<ProfilePictureUploadMessage> channel
) : IRequestHandler<UpdateProfilePictureCommand, Fin<LanguageExt.Unit>>
{
    public async Task<Fin<LanguageExt.Unit>> Handle(
        UpdateProfilePictureCommand request,
        CancellationToken cancellationToken
    )
    {
        var exists = await ctx.Scientists.AnyAsync(
            s => s.Id == request.ScientistId,
            cancellationToken
        );

        if (!exists)
        {
            return Error.New(StatusCodes.Status404NotFound, "Scientist not found");
        }

        var bytes = new byte[request.Image.Length];
        await using var readStream = request.Image.OpenReadStream();
        await readStream.ReadExactlyAsync(bytes, cancellationToken);
        await channel.Writer.WriteAsync(
            new ProfilePictureUploadMessage(request.ScientistId, bytes),
            cancellationToken
        );

        return Fin.Succ(Prelude.unit);
    }
}
