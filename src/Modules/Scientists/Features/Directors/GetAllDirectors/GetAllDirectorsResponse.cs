namespace Daab.Modules.Scientists.Features.Directors.GetAllDirectors;

public sealed record GetAllDirectorsResponse(IEnumerable<DirectorResponse> Directors);

public sealed record DirectorResponse(
    string Id,
    string? UserId,
    string? ProfilePictureUrl,
    string FirstName,
    string LastName,
    string Role,
    string AcademicTitle,
    string[] Countries
);
