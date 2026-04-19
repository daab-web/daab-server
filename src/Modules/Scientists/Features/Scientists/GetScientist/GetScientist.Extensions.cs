using Daab.Modules.Scientists.Models;

namespace Daab.Modules.Scientists.Features.Scientists.GetScientist;

public static class GetScientistExtensions
{
    extension(Scientist s)
    {
        public GetScientistResponse ToGetScientistResponse()
        {
            var t = s.Translations.First();
            return new GetScientistResponse(
                s.Id,
                s.UserId,
                s.Slug,
                t.FirstName!,
                t.LastName!,
                s.Email,
                t.Description,
                s.AcademicTitle,
                s.PhotoUrl,
                s.LinkedInUrl,
                s.Orcid,
                s.Website,
                s.DateOfBirth?.ToString("dd/MM/yyyy"),
                [.. s.Institutions],
                [.. s.Countries],
                [.. s.Areas]
            );
        }
    }
}
