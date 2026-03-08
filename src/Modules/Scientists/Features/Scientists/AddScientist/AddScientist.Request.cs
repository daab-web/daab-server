namespace Daab.Modules.Scientists.Features.Scientists.AddScientist;

public record AddScientistRequest(
    string? UserId,
    string FirstName,
    string LastName,
    string? Email,
    string? PhoneNumber,
    string Description,
    string AcademicTitle,
    string[] Institutions,
    string[] Countries,
    string[] Areas
);
