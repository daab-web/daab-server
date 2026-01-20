using Daab.Modules.Scientists.Persistence;
using MediatR;

namespace Daab.Modules.Scientists.Features.AddScientist;

public class AddScientistCommandHandler(ScientistsContext context)
    : IRequestHandler<AddScientistCommand, AddScientistResponse>
{
    public async Task<AddScientistResponse> Handle(
        AddScientistCommand request,
        CancellationToken cancellationToken
    )
    {
        var scientist = request.ToScientist();

        var e = await context.Scientists.AddAsync(scientist, cancellationToken: cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return e.Entity.ToAddResponse();
    }
}
