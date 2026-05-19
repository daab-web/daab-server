using Daab.Modules.Scientists.Models;
using MediatR;

namespace Daab.Modules.Scientists.Features.Directors.GetAllDirectors;

public class GetAllDirectorsQuery(string locale) : IRequest<IEnumerable<DirectorResponse>>
{
    public string Locale { get; init; } = locale;
}
