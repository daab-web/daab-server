using LanguageExt;
using MediatR;

namespace Daab.Modules.Scientists.Features.Publications.UpdatePublications;

public class UpdatePublicationCommand(UpdatePublicationRequest request)
    : IRequest<Fin<UpdatePublicationResponse>>
{
    public string Id { get; } = request.Id;
    public string Title { get; } = request.Title;
    public string? Url { get; } = request.Url;
}
