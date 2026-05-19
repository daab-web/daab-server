using LanguageExt;
using MediatR;

namespace Daab.Modules.Scientists.Features.Directors.CreateDirector;

public class CreateDirectorCommand(CreateDirectorRequest request)
    : IRequest<Fin<CreateDirectorResponse>>
{
    public string ScientistId { get; set; } = request.ScientistId;
    public readonly DirectorTranslationEntry[] Translations = request.Translations;
}
