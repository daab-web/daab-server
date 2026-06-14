using LanguageExt;
using MediatR;

namespace Daab.Modules.Scientists.Features.Scientists.DeleteProfilePicture;

public class DeleteProfilePictureCommand(string scientistId) : IRequest<Fin<bool>>
{
    public string ScientistId { get; } = scientistId;
}
