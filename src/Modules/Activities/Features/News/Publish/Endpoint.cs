using Daab.Modules.Activities.Persistence;
using Daab.Modules.Activities.Persistence.Migrations;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using EntityStatus = Daab.SharedKernel.Entities.EntityStatus;

namespace Daab.Modules.Activities.Features.News.Publish;

public sealed record PublishNewsRequest(List<string> NewsIds);

public class PublishNewsEndpoint(ActivitiesDbContext ctx) : Endpoint<PublishNewsRequest>
{
    public override void Configure()
    {
        Patch("/news");

        // TODO: This should not be public
        AllowAnonymous();
    }

    public override async Task HandleAsync(PublishNewsRequest req, CancellationToken ct)
    {
        if (req.NewsIds.Count == 0)
        {
            await Send.NoContentAsync(ct);
            return;
        }

        if (req.NewsIds.Count > 100)
        {
            ThrowError("Batch size cannot exceed 100", StatusCodes.Status400BadRequest);
            return;
        }

        await ctx
            .News.Where(n => req.NewsIds.Contains(n.Id) && n.Status == EntityStatus.ReadyToPublish)
            .ExecuteUpdateAsync(x => x.SetProperty(n => n.Status, EntityStatus.Published), ct);

        await Send.NoContentAsync(ct);
    }
}
