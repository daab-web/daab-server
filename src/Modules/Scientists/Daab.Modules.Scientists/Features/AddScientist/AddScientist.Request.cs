namespace Daab.Modules.Scientists.Features.AddScientist;

public record AddScientistRequest(
    string? UserId,
    string FullName,
    string Description,
    string AcademicTitle,
    string Institution,
    string[] Countries,
    string[] Areas
);

