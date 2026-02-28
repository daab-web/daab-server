using LanguageExt;
using MediatR;

namespace Daab.Modules.Scientists.Features.Scientists.DeleteScientist;

public class DeleteScientistCommand(string id) : IRequest<Fin<bool>>
{
    public string Id { get; set; } = id;
}
