using Daab.Modules.Scientists.Models;

namespace Daab.Modules.Scientists.Features.AddScientist;

public static class AddScientistExtensions
{
    extension(AddScientistCommand r)
    {
        public Scientist ToScientist()
        {
            return r.UserId is null
                ? new Scientist(
                    r.FullName,
                    r.Description,
                    r.AcademicTitle,
                    r.Institution,
                    r.Countries,
                    r.Areas
                )
                : new Scientist(
                    Guid.Parse(r.UserId),
                    r.FullName,
                    r.Description,
                    r.AcademicTitle,
                    r.Institution,
                    r.Countries,
                    r.Areas
                );
        }
    }

    extension(Scientist s)
    {
        public AddScientistResponse ToAddResponse()
        {
            return new AddScientistResponse(
                s.Id,
                s.UserId,
                s.FullName,
                s.Description,
                s.AcademicTitle,
                s.Institution,
                [.. s.Countries],
                [.. s.Areas]
            );
        }
    }
}

