using Daab.Modules.Scientists.Persistence;
using Daab.SharedKernel;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Daab.Modules.Scientists.Features.Scientists.DeleteProfilePicture;

public sealed class DeleteProfilePictureCommandHandler(
    ScientistsDbContext context,
    IBlobStorage blobStorage
) : IRequestHandler<DeleteProfilePictureCommand, Fin<bool>>
{
    public async Task<Fin<bool>> Handle(
        DeleteProfilePictureCommand request,
        CancellationToken cancellationToken
    )
    {
        var scientist = await context.Scientists.FindAsync(
            [request.ScientistId],
            cancellationToken
        );

        if (scientist is null)
        {
            return Error.New(
                StatusCodes.Status404NotFound,
                $"Scientist with an Id of {request.ScientistId} not found"
            );
        }

        await blobStorage.DeleteAsync(
            "scientists",
            $"profile-pictures/{request.ScientistId}.webp",
            cancellationToken
        );

        scientist.PhotoUrl = null;
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
