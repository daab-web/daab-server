namespace Daab.Modules.Scientists.Features.Apply;

public sealed record ApplyRequest(
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
    string? AdditionalInformationToShare
);
