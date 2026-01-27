namespace Daab.Modules.Scientists.Models;

public class Scientist
{
    public string Id { get; private set; }
    public string? UserId { get; private set; }
    public string Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Description { get; private set; }
    public string AcademicTitle { get; private set; }
    public string Institution { get; private set; }

    public IEnumerable<string> Countries { get; private set; }
    public IEnumerable<string> Areas { get; private set; }

    public string Slug()
    {
        var name = FirstName.Replace(' ', '-');
        var lastName = LastName.Replace(' ', '-');
        return $"{name}-{lastName}-{Id[..5]}";
    }

    public Scientist(
        string firstName,
        string lastName,
        string email,
        string? phoneNumber,
        string description,
        string academicTitle,
        string institution,
        IEnumerable<string> countries,
        IEnumerable<string> areas
    )
    {
        Id = Ulid.NewUlid().ToString();
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        Description = description;
        AcademicTitle = academicTitle;
        Institution = institution;
        Countries = countries;
        Areas = areas;
    }

    public Scientist(
        Ulid userId,
        string firstName,
        string lastName,
        string email,
        string? phoneNumber,
        string description,
        string academicTitle,
        string institution,
        IEnumerable<string> countries,
        IEnumerable<string> areas
    )
        : this(
            firstName,
            lastName,
            email,
            phoneNumber,
            description,
            academicTitle,
            institution,
            countries,
            areas
        )
    {
        UserId = userId.ToString();
    }
}
