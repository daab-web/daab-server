using Daab.SharedKernel.Middlewares;
using Microsoft.AspNetCore.Http;

namespace Daab.Modules.Scientists.Features.Scientists.AddScientist;

public record AddScientistRequest(
    string Locale,
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
    CreatePublicationDto[]? Publications,
    DateTime? DateOfBirth
) : ILocalized;

public sealed record CreatePublicationDto(string Title, string? Url);
