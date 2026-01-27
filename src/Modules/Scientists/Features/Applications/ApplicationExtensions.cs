using Daab.Modules.Scientists.Features.GetAllApplications;
using Daab.Modules.Scientists.Models;
using LanguageExt;

namespace Daab.Modules.Scientists.Features.Applications;

public static class ApplicationExtensions
{
    extension(Application a)
    {
        public ApplicationDto ToDto()
        {
            return new ApplicationDto(
                a.Id,
                a.Email,
                a.Email,
                a.Surname,
                a.Residence,
                a.City,
                a.PhoneNumber,
                a.UniversityName,
                a.FieldOfStudy,
                a.AcademicDegree,
                a.AlmaMater,
                a.AcademicTitle,
                a.DegreeInstitution,
                a.JobPosition,
                a.PreviousJob,
                a.ContributionsToDaab,
                a.EngagedScientistFields,
                a.AdditionalInformation,
                a.AdditionalInformationToShare,
                a.PhotoUrl,
                a.CvUrl,
                a.Status.ToString()
            );
        }
    }

    extension(IQueryable<Application> applications)
    {
        public IQueryable<ApplicationDto> ToDto()
        {
            return applications.Select(a => a.ToDto());
        }
    }
}
