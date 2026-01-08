namespace Daab.Modules.Scientists.Features.AddScientist;

public record AddScientistResponse(
    string Id,
    string? UserId,
    string FullName,
    string Description,
    string AcademicTitle,
    string Institution,
    string[] Countries,
    string[] Areas
);


