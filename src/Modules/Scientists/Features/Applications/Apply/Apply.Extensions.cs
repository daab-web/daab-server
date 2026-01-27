using Daab.Modules.Scientists.Features.Applications.Apply;
using Daab.Modules.Scientists.Models;

namespace Daab.Modules.Scientists.Features.Apply;

public static class ApplyExtensions
{
    extension(ApplyRequest r)
    {
        public Application ToEntity()
        {
            return new Application(
                r.Email,
                r.Name,
                r.Surname,
                r.Residence,
                r.City,
                r.PhoneNumber,
                r.UniversityName,
                r.FieldOfStudy,
                r.AcademicDegree,
                r.AlmaMater,
                r.AcademicTitle,
                r.DegreeInstitution,
                r.ContributionsToDaab
            )
            {
                JobPosition = r.JobPosition,
                PreviousJob = r.PreviousJob,
                EngagedScientistFields = r.EngagedScientistFields,
                AdditionalInformation = r.AdditionalInformation,
                AdditionalInformationToShare = r.AdditionalInformationToShare,
            };
        }
    }
}
