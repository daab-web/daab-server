namespace Daab.Modules.Scientists.Models;

public class Application : BaseEntity
{
    public string Email { get; init; }
    public string Name { get; init; }
    public string Surname { get; init; }
    public string Residence { get; init; }
    public string City { get; init; }
    public string PhoneNumber { get; init; }
    public string UniversityName { get; init; }
    public string FieldOfStudy { get; init; }
    public string AcademicDegree { get; init; }
    public string AlmaMater { get; init; }
    public string AcademicTitle { get; init; }
    public string DegreeInstitution { get; init; }
    public string? JobPosition { get; init; }
    public string? PreviousJob { get; init; }
    public string ContributionsToDaab { get; init; }
    public string? EngagedScientistFields { get; init; }
    public string? AdditionalInformation { get; init; }
    public string? AdditionalInformationToShare { get; init; }
    public string? PhotoUrl { get; init; }
    public string? CvUrl { get; init; }

    public Application(
        string email,
        string name,
        string surname,
        string residence,
        string city,
        string phoneNumber,
        string universityName,
        string fieldOfStudy,
        string academicDegree,
        string almaMater,
        string academicTitle,
        string degreeInstitution,
        string contributionsToDaab
    )
    {
        Email = email;
        Name = name;
        Surname = surname;
        Residence = residence;
        City = city;
        PhoneNumber = phoneNumber;
        UniversityName = universityName;
        FieldOfStudy = fieldOfStudy;
        AcademicDegree = academicDegree;
        AlmaMater = almaMater;
        AcademicTitle = academicTitle;
        DegreeInstitution = degreeInstitution;
        ContributionsToDaab = contributionsToDaab;
    }
}
