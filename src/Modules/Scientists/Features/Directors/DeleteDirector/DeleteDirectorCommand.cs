using LanguageExt;
using MediatR;

namespace Daab.Modules.Scientists.Features.Directors.DeleteDirector;

public class DeleteDirectorCommand(string id) : IRequest<Fin<bool>>
{
    public string Id { get; set; } = id;
}
