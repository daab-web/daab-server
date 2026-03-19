using Daab.Modules.Scientists.Models;

namespace Daab.Modules.Scientists.Features.Scientists.GetScientist;

public static class GetScientistExtensions
{
    extension(Scientist s)
    {
        public GetScientistResponse ToGetScientistResponse()
        {
            return new GetScientistResponse(
                s.Id,
                s.UserId,
                s.Slug,
                s.FirstName,
                s.LastName,
                s.Email,
                s.Description,
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
