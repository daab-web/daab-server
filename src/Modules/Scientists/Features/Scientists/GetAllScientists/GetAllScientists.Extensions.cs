using Daab.Modules.Scientists.Models;

namespace Daab.Modules.Scientists.Features.Scientists.GetAllScientists;

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
                s.FirstName,
                s.LastName,
                s.Description,
                s.AcademicTitle,
                [.. s.Institutions],
                [.. s.Countries],
                [.. s.Areas]
            );
        }
    }

    extension(IQueryable<Scientist> scientists)
    {
        public IQueryable<GetAllScientistsResponse> ToAllScientistsResponse() =>
            scientists.Select(s => s.ToAllScientistsResponse());
    }
}
