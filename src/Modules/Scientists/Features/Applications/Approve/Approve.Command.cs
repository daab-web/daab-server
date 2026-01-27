using LanguageExt;
using MediatR;

namespace Daab.Modules.Scientists.Features.Applications.Approve;

public sealed record ApproveApplicationCommand : IRequest<Fin<ApproveApplicationResponse>>
{
    public string ApplicationId { get; }

    public ApproveApplicationCommand(string applicationId)
    {
        ApplicationId = applicationId;
    }
}
