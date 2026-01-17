using FastEndpoints;
using MediatR;

namespace Daab.Modules.Scientists.Features.AddScientist;

public class AddScientistEndpoint(IMediator mediator)
    : Endpoint<AddScientistRequest, AddScientistResponse>
{
    public override void Configure()
    {
        Post("/scientists");
        AllowAnonymous();
    }

    public override async Task HandleAsync(AddScientistRequest req, CancellationToken ct)
    {
        var response = await mediator.Send(new AddScientistCommand(req), ct);

        await Send.CreatedAtAsync<AddScientistEndpoint>(
            responseBody: response,
            verb: Http.POST,
            generateAbsoluteUrl: true,
            cancellation: ct
        );
    }
}
