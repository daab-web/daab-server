namespace Daab.Modules.Scientists.Features.Scientists.GetAllScientists;

public sealed record GetAllScientistsResponse(
    string Id,
    string? UserId,
    string Slug,
    string FirstName,
    string LastName,
    string? Description,
    string AcademicTitle,
    string Institution,
    string[] Countries,
    string[] Areas
);
