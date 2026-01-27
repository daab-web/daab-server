namespace Daab.Modules.Scientists.Features.GetAllApplications;

public sealed record ApplicationDto(
    string Id,
    string Email,
    string Name,
    string Surname,
    string Residence,
    string City,
    string PhoneNumber,
    string UniversityName,
    string FieldOfStudy,
    string AcademicDegree,
    string AlmaMater,
    string AcademicTitle,
    string DegreeInstitution,
    string? JobPosition,
    string? PreviousJob,
    string ContributionsToDaab,
    string? EngagedScientistFields,
    string? AdditionalInformation,
    string? AdditionalInformationToShare,
    string? PhotoUrl,
    string? CvUrl,
    string Status
);
