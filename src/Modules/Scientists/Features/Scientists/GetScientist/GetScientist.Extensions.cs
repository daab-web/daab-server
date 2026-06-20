using Daab.Modules.Scientists.Models;

namespace Daab.Modules.Scientists.Features.Scientists.GetScientist;

public static class GetScientistExtensions
{
    extension(Scientist s)
    {
        public GetScientistResponse ToGetScientistResponse()
        {
            var t = s.Translations.FirstOrDefault();
            return new GetScientistResponse(
                s.Id,
                s.UserId,
                s.Slug,
                t?.FirstName ?? "Untranslated",
                t?.LastName ?? "Untranslated",
                s.Email,
                t?.Description,
                s.AcademicTitle,
                s.PhotoUrl,
                s.LinkedInUrl,
                s.Orcid,
                s.Website,
                s.DateOfBirth?.ToString("yyyy-MM-dd"),
                [.. s.Institutions],
                [.. s.Countries],
                [.. s.Areas]
            );
        }
    }
}
