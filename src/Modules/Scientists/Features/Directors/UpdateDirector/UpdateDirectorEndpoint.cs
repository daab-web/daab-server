using Daab.Modules.Scientists.Persistence;
using Daab.SharedKernel.Extensions;
using Daab.SharedKernel.Middlewares;
using FastEndpoints;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Daab.Modules.Scientists.Features.Directors.UpdateDirector;

public sealed record UpdateDirectorRequest(string Locale, string DirectorId, string Role)
    : ILocalizedCollection
{
    public IEnumerable<string> GetLocales() => [Locale];
}

public class UpdateDirectorEndpoint(IMediator mediator) : Endpoint<UpdateDirectorRequest>
{
    public override void Configure()
    {
        Put("/directors/{directorId}/{locale}");
        PreProcessor<LocaleCollectionPreProcessor>();

        // TODO: This should not be public
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateDirectorRequest req, CancellationToken ct)
    {
        var res = await mediator.Send(new UpdateDirectorCommand(req), ct);

        await res.Match(
            _ => Send.NoContentAsync(ct),
            err => err.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
        );
    }
}

public sealed class UpdateDirectorCommand(UpdateDirectorRequest req) : IRequest<Fin<string>>
{
    public string Locale { get; } = req.Locale;
    public string DirectorId { get; } = req.DirectorId;
    public string Role { get; } = req.Role;
}

public sealed class UpdateDirectorCommandHandler(ScientistsDbContext ctx)
    : IRequestHandler<UpdateDirectorCommand, Fin<string>>
{
    public async Task<Fin<string>> Handle(
        UpdateDirectorCommand request,
        CancellationToken cancellationToken
    )
    {
        var director = await ctx.Directors.FindAsync(
            [request.DirectorId],
            cancellationToken: cancellationToken
        );

        if (director is null)
        {
            return Error.New(StatusCodes.Status404NotFound, "Requested director not found");
        }

        director.RoleTranslations[request.Locale] = request.Role;
        ctx.Entry(director).Property(d => d.RoleTranslations).IsModified = true;
        await ctx.SaveChangesAsync(cancellationToken);

        return director.Id;
    }
}
