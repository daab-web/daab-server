namespace Daab.Modules.Scientists.Features.Publications.UpdatePublications;

public sealed record UpdatePublicationRequest(string Id, string Title, string? Url);
