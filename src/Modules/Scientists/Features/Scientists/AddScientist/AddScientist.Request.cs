namespace Daab.Modules.Scientists.Features.AddScientist;

public record AddScientistRequest(
    string? UserId,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string Description,
    string AcademicTitle,
    string Institution,
    string[] Countries,
    string[] Areas
);
