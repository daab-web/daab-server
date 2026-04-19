using Daab.Modules.Scientists.Models;
using Daab.Modules.Scientists.Persistence;
using Daab.SharedKernel.Extensions;
using Daab.SharedKernel.Middlewares;
using FastEndpoints;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Unit = LanguageExt.Unit;

namespace Daab.Modules.Scientists.Features.Scientists.AddTranslation;

public sealed record AddTranslationRequest(
    string Locale,
    string ScientistId,
    string FirstName,
    string LastName,
    string Description
) : ILocalized;

public sealed class AddTranslationCommand(AddTranslationRequest req) : IRequest<Fin<Unit>>
{
    public string Locale { get; } = req.Locale;
    public string ScientistId { get; } = req.ScientistId;
    public string FirstName { get; } = req.FirstName;
    public string LastName { get; } = req.LastName;
    public string Description { get; } = req.Description;
}

public sealed class AddTranslationCommandHandler(ScientistsDbContext ctx)
    : IRequestHandler<AddTranslationCommand, Fin<Unit>>
{
    public async Task<Fin<Unit>> Handle(
        AddTranslationCommand request,
        CancellationToken cancellationToken
    )
    {
        var translation = ctx.ScientistTranslations.SingleOrDefault(st =>
            st.ScientistId == request.ScientistId && st.Locale == request.Locale
        );

        if (translation is null)
        {
            await ctx.ScientistTranslations.AddAsync(
                new ScientistTranslation
                {
                    Locale = request.Locale,
                    ScientistId = request.ScientistId,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Description = request.LastName,
                },
                cancellationToken
            );
        }
        else
        {
            translation.Update(request.FirstName, request.LastName, request.Description);
        }

        var statesWritten = await ctx.SaveChangesAsync(cancellationToken);

        return statesWritten > 0 ? Prelude.unit : Error.New(500, "Unable to add translation");
    }
}

public class AddTranslationEndpoint(IMediator mediator) : Endpoint<AddTranslationRequest>
{
    public override void Configure()
    {
        Post("/scientists/{scientistId}/translations");
        AllowAnonymous();
    }

    public override async Task HandleAsync(AddTranslationRequest req, CancellationToken ct)
    {
        var res = await mediator.Send(new AddTranslationCommand(req), cancellationToken: ct);

        await res.Match(
            _ => Send.NoContentAsync(ct),
            err => err.ToProblemDetails(HttpContext).ExecuteAsync(HttpContext)
        );
    }
}
