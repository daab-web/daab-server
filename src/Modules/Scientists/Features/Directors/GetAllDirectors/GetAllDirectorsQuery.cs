using Daab.Modules.Scientists.Models;
using MediatR;

namespace Daab.Modules.Scientists.Features.Directors.GetAllDirectors;

public class GetAllDirectorsQuery : IRequest<IEnumerable<Director>>;
