using MediatR;

namespace Daab.Modules.Scientists.Features.Scientists.GetScientist;

public record GetScientistQuery : IRequest<GetScientistResponse>
{
    public string IdOrSlug { get; }
    public string Locale { get; }

    public GetScientistQuery(string idOrSlug, string locale)
    {
        IdOrSlug = idOrSlug;
        Locale = locale;
    }
}
