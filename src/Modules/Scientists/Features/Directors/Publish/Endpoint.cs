using Daab.Modules.Scientists.Persistence;
using Daab.SharedKernel.Entities;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Scientists.Features.Directors.Publish;

public record PublishDirectorRequest(List<string> DirectorIds);

public class PublishDirectorEndpoint(ScientistsDbContext ctx) : Endpoint<PublishDirectorRequest>
{
    public override void Configure()
    {
        Patch("/directors");

        // TODO: This should not be public
        AllowAnonymous();
    }

    public override async Task HandleAsync(PublishDirectorRequest req, CancellationToken ct)
    {
        if (req.DirectorIds.Count == 0)
        {
            await Send.NoContentAsync(ct);
            return;
        }

        if (req.DirectorIds.Count > 100)
        {
            ThrowError("Batch size cannot exceed 100", StatusCodes.Status400BadRequest);
            return;
        }

        await ctx
            .Directors.Where(d =>
                req.DirectorIds.Contains(d.Id) && d.Status == EntityStatus.ReadyToPublish
            )
            .ExecuteUpdateAsync(x => x.SetProperty(d => d.Status, EntityStatus.Published), ct);

        await Send.NoContentAsync(ct);
    }
}
