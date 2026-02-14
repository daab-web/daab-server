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
                s.Slug(),
                s.FirstName,
                s.LastName,
                s.Description,
                s.AcademicTitle,
                s.Institution,
                [.. s.Countries],
                [.. s.Areas]
            );
        }
    }
}
