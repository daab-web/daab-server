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

        var e = await context.Scientists.AddAsync(scientist, cancellationToken: cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return e.Entity;
    }
}
