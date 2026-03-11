using Daab.Modules.Scientists.Models;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Daab.Modules.Scientists.Features.Scientists.AddScientist;

public class AddScientistEndpoint(IMediator mediator)
    : EndpointWithMapping<AddScientistRequest, AddScientistResponse, Scientist>
{
    public override void Configure()
    {
        Post("/scientists");
        AllowFormData();
        AllowAnonymous();
    }

    public override async Task HandleAsync(AddScientistRequest req, CancellationToken ct)
    {
        var scientist = MapToEntity(req);

        if (req.Photo?.ContentType.StartsWith("image/") == false)
        {
            await Send.ResultAsync(
                TypedResults.BadRequest(
                    "Invalid profile picture format. Only image files are allowed."
                )
            );
            return;
        }

        var entity = await mediator.Send(new AddScientistCommand(scientist, req.Photo), ct);

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
        List<Publication> publications = [];

        var scientist = new Scientist(
            r.FirstName,
            r.LastName,
            r.Email,
            r.PhoneNumber,
            r.Description,
            r.AcademicTitle,
            r.Institutions,
            r.Countries,
            r.Areas,
            null,
            r.LinkedInUrl,
            r.Orcid,
            r.Website
        )
        {
            Publications = publications,
            Slug = string.Empty,
        };

        if (r.Publications is not null)
        {
            publications.AddRange(
                r.Publications.Select(p => new Publication(p.Url)
                {
                    Title = p.Title,
                    ScientistId = scientist.Id,
                })
            );
        }

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
