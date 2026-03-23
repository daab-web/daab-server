using MediatR;

namespace Daab.Modules.Scientists.Features.Applications.Reject;

public class RejectApplicationCommand(string applicationId) : IRequest
{
    public string ApplicationId { get; } = applicationId;
}
