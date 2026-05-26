using Daab.Modules.Scientists.Persistence;
using Daab.SharedKernel.Entities;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Scientists.Features.Scientists.Publish;

public sealed record PublishScientistRequest(List<string> ScientistIds);

public class PublishScientistEndpoint(ScientistsDbContext ctx) : Endpoint<PublishScientistRequest>
{
    public override void Configure()
    {
        Patch("/scientists");

        // TODO: This should not be public
        AllowAnonymous();
    }

    public override async Task HandleAsync(PublishScientistRequest req, CancellationToken ct)
    {
        if (req.ScientistIds.Count == 0)
        {
            await Send.NoContentAsync(ct);
            return;
        }

        if (req.ScientistIds.Count > 100)
        {
            ThrowError("Batch size cannot exceed 100", StatusCodes.Status400BadRequest);
            return;
        }

        await ctx
            .Scientists.Where(s =>
                req.ScientistIds.Contains(s.Id) && s.Status == EntityStatus.ReadyToPublish
            )
            .ExecuteUpdateAsync(
                x => x.SetProperty(s => s.Status, EntityStatus.Published),
                cancellationToken: ct
            );

        await Send.NoContentAsync(ct);
    }
}
