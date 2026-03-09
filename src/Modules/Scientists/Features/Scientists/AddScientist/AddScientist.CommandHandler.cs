using Daab.Modules.Scientists.Models;
using Daab.Modules.Scientists.Persistence;
using MediatR;

namespace Daab.Modules.Scientists.Features.Scientists.AddScientist;

public class AddScientistCommandHandler(ScientistsDbContext context)
    : IRequestHandler<AddScientistCommand, Scientist>
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
            return e.Entity;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
