namespace Daab.Modules.Scientists.Features.Scientists.AddScientist;

public record AddScientistResponse(
    string Id,
    string? UserId,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string? Description,
    string AcademicTitle,
    string[] Institution,
    string[] Countries,
    string[] Areas
);
