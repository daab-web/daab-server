using LanguageExt;
using MediatR;

namespace Daab.Modules.Scientists.Features.Directors.CreateDirector;

public class CreateDirectorCommand(CreateDirectorRequest request)
    : IRequest<Fin<CreateDirectorResponse>>
{
    public string Role { get; set; } = request.Role;
    public string ScientistId { get; set; } = request.ScientistId;
}
