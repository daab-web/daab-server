using Daab.Modules.Scientists.Models;

namespace Daab.Modules.Scientists.Features.GetAllScientists;

public static class GetAllScientists
{
    extension(Scientist s)
    {
        public GetAllScientistsResponse ToAllScientistsResponse()
        {
            return new GetAllScientistsResponse(
                s.Id,
                s.UserId,
                s.Slug(),
                s.FullName,
                s.Description,
                s.AcademicTitle,
                s.Institution,
                [.. s.Countries],
                [.. s.Areas]
            );
        }
    }

    extension(IEnumerable<Scientist> scientists)
    {
        public IEnumerable<GetAllScientistsResponse> ToAllScientistsResponse()
            => scientists.Select(s => s.ToAllScientistsResponse());
    }
}
