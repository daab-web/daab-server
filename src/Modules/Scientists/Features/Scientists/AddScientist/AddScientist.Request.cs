using Microsoft.AspNetCore.Http;

namespace Daab.Modules.Scientists.Features.Scientists.AddScientist;

public record AddScientistRequest(
    string? UserId,
    string FirstName,
    string LastName,
    string? Email,
    string? PhoneNumber,
    string Description,
    string AcademicTitle,
    IFormFile? Photo,
    string? LinkedInUrl,
    string? Orcid,
    string? Website,
    string[] Institutions,
    string[] Countries,
    string[] Areas,
    CreatePublicationDto[]? Publications
);

public sealed record CreatePublicationDto(String Title, string? Url);
