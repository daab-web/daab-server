namespace Daab.Modules.Scientists.Features.Scientists.GetScientist;

public record GetScientistResponse(
    string Id,
    string? UserId,
    string Slug,
    string FirstName,
    string LastName,
    string? Description,
    string AcademicTitle,
    string[] Institutions,
    string[] Countries,
    string[] Areas
);
