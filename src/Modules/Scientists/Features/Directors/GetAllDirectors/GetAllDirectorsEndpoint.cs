using Daab.Modules.Scientists.Models;
using Daab.SharedKernel.Middlewares;
using FastEndpoints;
using MediatR;

namespace Daab.Modules.Scientists.Features.Directors.GetAllDirectors;

public record GetAllDirectorsRequest(string Locale) : ILocalized;

public class GetAllDirectorsEndpoint(IMediator mediator)
    : EndpointWithMapping<GetAllDirectorsRequest, GetAllDirectorsResponse, IEnumerable<Director>>
{
    public override void Configure()
    {
        Get("/directors");
        PreProcessor<LocalePreProcessor>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetAllDirectorsRequest req, CancellationToken ct)
    {
        var data = await mediator.Send(new GetAllDirectorsQuery(req.Locale), ct);
        await Send.OkAsync(MapFromEntity(data), ct);
    }

    public override GetAllDirectorsResponse MapFromEntity(IEnumerable<Director> directors)
    {
        var data = directors.Select(director =>
        {
            if (director.Scientist is null)
            {
                throw new InvalidOperationException(
                    $"Scientist not loaded for director {director.Id}. Use Include()"
                );
            }

            var t = director.Scientist.Translations.First();

            return new DirectorResponse(
                director.Id,
                director.ScientistId,
                director.Scientist.PhotoUrl,
                t.FirstName!,
                t.LastName!,
                director.Role,
                director.Scientist.AcademicTitle,
                [.. director.Scientist.Countries]
            );
        });
        return new GetAllDirectorsResponse(data);
    }
}
