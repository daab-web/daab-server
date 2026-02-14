using LanguageExt;
using MediatR;

namespace Daab.Modules.Scientists.Features.Applications.GetApplication;

public sealed record GetApplicationQuery : IRequest<Fin<ApplicationDto>>
{
    public GetApplicationQuery(string applicationId)
    {
        ApplicationId = applicationId;
    }

    public string ApplicationId { get; }
}
