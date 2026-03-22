using Daab.Modules.Scientists.Models;
using FastEndpoints;
using MediatR;

namespace Daab.Modules.Scientists.Features.Directors.GetAllDirectors;

public class GetAllDirectorsEndpoint(IMediator mediator)
    : EndpointWithMapping<EmptyRequest, GetAllDirectorsResponse, IEnumerable<Director>>
{
    public override void Configure()
    {
        Get("/directors");
        AllowAnonymous();
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        var data = await mediator.Send(new GetAllDirectorsQuery(), ct);
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

            return new DirectorResponse(
                director.Id,
                director.ScientistId,
                director.Scientist.PhotoUrl,
                director.Scientist.FirstName,
                director.Scientist.LastName,
                director.Role,
                director.Scientist.AcademicTitle,
                [.. director.Scientist.Countries]
            );
        });
        return new GetAllDirectorsResponse(data);
    }
}
