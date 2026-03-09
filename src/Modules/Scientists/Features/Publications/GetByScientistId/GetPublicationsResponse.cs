namespace Daab.Modules.Scientists.Features.Publications.GetByScientistId;

public sealed record GetPublicationsResponse(GetPublicationDto[] Publications);

public sealed record GetPublicationDto(string Id, string Title, string? Url, string ScientistId);
