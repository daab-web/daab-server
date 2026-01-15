namespace Daab.Modules.Scientists.Features.GetAllScientists;

public sealed record GetAllScientistsResponse(
    string Id,
    string? UserId,
    string Slug,
    string Fullname,
    string Description,
    string AcademicTitle,
    string Institution,
    string[] Countries,
    string[] Areas
);
