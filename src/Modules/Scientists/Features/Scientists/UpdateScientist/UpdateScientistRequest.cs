namespace Daab.Modules.Scientists.Features.Scientists.UpdateScientist;

public sealed record UpdateScientistRequest(
    string? Email,
    string? PhoneNumber,
    string FirstName,
    string LastName,
    string? Description,
    string AcademicTitle,
    string? LinkedInUrl,
    string? Orcid,
    string? Website,
    string[] Institutions,
    string[] Countries,
    string[] Areas
);
