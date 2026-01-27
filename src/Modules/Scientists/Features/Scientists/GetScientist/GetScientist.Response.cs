namespace Daab.Modules.Scientists.Features.GetScientist;

public record GetScientistResponse(
    string Id,
    string? UserId,
    string Slug,
    string FirstName,
    string LastName,
    string Description,
    string AcademicTitle,
    string Institution,
    string[] Countries,
    string[] Areas
);


