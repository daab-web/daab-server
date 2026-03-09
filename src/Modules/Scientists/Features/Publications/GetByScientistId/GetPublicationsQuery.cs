using MediatR;

namespace Daab.Modules.Scientists.Features.Publications.GetByScientistId;

public class GetPublicationsQuery : IRequest<GetPublicationsResponse>
{
    public GetPublicationsQuery(string scientistId)
    {
        ScientistId = scientistId;
    }

    public string ScientistId { get; set; }
}
