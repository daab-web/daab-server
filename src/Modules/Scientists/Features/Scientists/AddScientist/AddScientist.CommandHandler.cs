using System.Threading.Channels;
using Daab.Modules.Scientists.Messages;
using Daab.Modules.Scientists.Models;
using Daab.Modules.Scientists.Persistence;
using Daab.SharedKernel;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Daab.Modules.Scientists.Features.Scientists.AddScientist;

public class AddScientistCommandHandler(
    ScientistsDbContext context,
    [FromKeyedServices(ChannelKeys.ProfilePictureUpload)]
        Channel<ProfilePictureUploadMessage> channel
) : IRequestHandler<AddScientistCommand, Scientist>
{
    public async Task<Scientist> Handle(
        AddScientistCommand request,
        CancellationToken cancellationToken
    )
    {
        var scientist = request.Scientist;
        var publications = request.Scientist.Publications;

        await using var transaction = await context.Database.BeginTransactionAsync(
            cancellationToken
        );

        try
        {
            await context.Publications.AddRangeAsync(publications, cancellationToken);
            var e = await context.Scientists.AddAsync(
                scientist,
                cancellationToken: cancellationToken
            );
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            await using var stream = new MemoryStream();

            if (request.Photo is null)
            {
                return e.Entity;
            }

            await request.Photo.CopyToAsync(stream, cancellationToken);
            stream.Position = 0;

            var message = new ProfilePictureUploadMessage(e.Entity.Id, stream.ToArray());

            await channel.Writer.WriteAsync(message, cancellationToken);

            return e.Entity;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
