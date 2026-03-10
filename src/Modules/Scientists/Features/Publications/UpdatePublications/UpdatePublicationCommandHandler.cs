using Daab.Modules.Scientists.Persistence;
using LanguageExt;
using LanguageExt.Common;
using MediatR;

namespace Daab.Modules.Scientists.Features.Publications.UpdatePublications;

public class UpdatePublicationCommandHandler(ScientistsDbContext context)
    : IRequestHandler<UpdatePublicationCommand, Fin<UpdatePublicationResponse>>
{
    public async Task<Fin<UpdatePublicationResponse>> Handle(
        UpdatePublicationCommand request,
        CancellationToken cancellationToken
    )
    {
        var publication = await context.Publications.FindAsync([request.Id], cancellationToken);
        if (publication is null)
        {
            return Error.New(400, $"Publication with an Id of {request.Id} not found");
        }

        publication.Url = request.Url;
        publication.Title = request.Title;

        await context.SaveChangesAsync(cancellationToken);
        return new UpdatePublicationResponse(request.Id);
    }
}
