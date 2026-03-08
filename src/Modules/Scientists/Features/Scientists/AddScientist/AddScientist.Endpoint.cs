using Daab.Modules.Scientists.Models;
using FastEndpoints;
using MediatR;

namespace Daab.Modules.Scientists.Features.Scientists.AddScientist;

public class AddScientistEndpoint(IMediator mediator)
    : EndpointWithMapping<AddScientistRequest, AddScientistResponse, Scientist>
{
    public override void Configure()
    {
        Post("/scientists");
        AllowAnonymous();
    }

    public override async Task HandleAsync(AddScientistRequest req, CancellationToken ct)
    {
        var scientist = MapToEntity(req);
        var entity = await mediator.Send(new AddScientistCommand(scientist), ct);

        var response = MapFromEntity(entity);

        await Send.CreatedAtAsync<AddScientistEndpoint>(
            responseBody: response,
            verb: Http.POST,
            generateAbsoluteUrl: true,
            cancellation: ct
        );
    }

    public override Scientist MapToEntity(AddScientistRequest r)
    {
        var scientist = new Scientist(
            r.FirstName,
            r.LastName,
            r.Email,
            r.PhoneNumber,
            r.Description,
            r.AcademicTitle,
            r.Institutions,
            r.Countries,
            r.Areas
        );

        if (r.UserId is not null)
        {
            scientist.LinkUser(r.UserId);
        }

        return scientist;
    }

    public override AddScientistResponse MapFromEntity(Scientist e)
    {
        return new AddScientistResponse(e.Id);
    }
}
