using Daab.Modules.Scientists.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Scientists.Features.Applications.Reject;

public class RejectApplicationCommandHandler(ScientistsDbContext context)
    : IRequestHandler<RejectApplicationCommand>
{
    public async Task Handle(RejectApplicationCommand request, CancellationToken cancellationToken)
    {
        var application = await context.Applications.SingleOrDefaultAsync(
            a => a.Id == request.ApplicationId,
            cancellationToken: cancellationToken
        );

        if (application is null)
        {
            return;
        }
        context.Applications.Remove(application);
        await context.SaveChangesAsync(cancellationToken);
    }
}
