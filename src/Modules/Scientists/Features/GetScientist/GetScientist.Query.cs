using MediatR;

namespace Daab.Modules.Scientists.Features.GetScientist;

public record GetScientistQuery : IRequest<GetScientistResponse>
{
    public string IdOrSlug { get; }

    public GetScientistQuery(string idOrSlug)
    {
        IdOrSlug = idOrSlug;
    }
}

