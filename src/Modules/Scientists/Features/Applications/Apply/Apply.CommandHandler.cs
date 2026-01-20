using Daab.Modules.Scientists.Persistence;
using Grpc.Core;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Daab.Modules.Scientists.Features.Apply;

public sealed class ApplyCommandHandler(ScientistsContext context)
    : IRequestHandler<ApplyCommand, Fin<ApplyResponse>>
{
    public async Task<Fin<ApplyResponse>> Handle(
        ApplyCommand request,
        CancellationToken cancellationToken
    )
    {
        var application = request.Data.ToEntity();

        var entityEntry = await context.Applications.AddAsync(application, cancellationToken);
        var entriesAffected = await context.SaveChangesAsync(cancellationToken);

        if (entriesAffected <= 0)
        {
            return Error.New(StatusCodes.Status500InternalServerError, "Unable to apply. Please try again later");
        }

        return new ApplyResponse(entityEntry.Entity.Id, true);
    }
}
