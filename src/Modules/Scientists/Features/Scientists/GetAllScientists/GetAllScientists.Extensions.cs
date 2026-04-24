using Daab.Modules.Scientists.Models;

namespace Daab.Modules.Scientists.Features.Scientists.GetAllScientists;

public static class GetAllScientists
{
    extension(Scientist s)
    {
        public GetAllScientistsResponse ToAllScientistsResponse(string locale)
        {
            var t = s.Translations.First(t => t.Locale == locale);
            return new GetAllScientistsResponse(
                s.Id,
                s.UserId,
                s.Slug,
                t.FirstName!,
                t.LastName!,
                t.Description,
                s.AcademicTitle,
                [.. s.Institutions],
                [.. s.Countries],
                [.. s.Areas]
            );
        }
    }

    extension(IEnumerable<Scientist> scientists)
    {
        public List<GetAllScientistsResponse> ToAllScientistsResponse(string locale) =>
            [.. scientists.Select(s => s.ToAllScientistsResponse(locale))];
    }
}
