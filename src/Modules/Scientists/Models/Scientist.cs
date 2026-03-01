using System.ComponentModel.DataAnnotations;

namespace Daab.Modules.Scientists.Models;

public class Scientist
{
    [MaxLength(36)]
    public string Id { get; init; } = Ulid.NewUlid().ToString();

    [MaxLength(36)]
    public string? UserId { get; init; }

    [MaxLength(320)]
    public string Email { get; set; }

    [MaxLength(15)]
    public string? PhoneNumber { get; set; }

    [MaxLength(64)]
    public string FirstName { get; set; }

    [MaxLength(64)]
    public string LastName { get; set; }

    public string? Description { get; set; }

    [MaxLength(320)]
    public string AcademicTitle { get; set; }

    public ICollection<string> Institutions { get; set; }
    public ICollection<string> Countries { get; set; }
    public ICollection<string> Areas { get; set; }

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
        string? description,
        string academicTitle,
        ICollection<string> institutions,
        ICollection<string> countries,
        ICollection<string> areas
    )
    {
        Email = email;
        PhoneNumber = phoneNumber;
        FirstName = firstName;
        LastName = lastName;
        Description = description;
        AcademicTitle = academicTitle;
        Institutions = institutions;
        Countries = countries;
        Areas = areas;
    }

    public Scientist(
        Ulid userId,
        string firstName,
        string lastName,
        string email,
        string? phoneNumber,
        string? description,
        string academicTitle,
        ICollection<string> institutions,
        ICollection<string> countries,
        ICollection<string> areas
    )
        : this(
            firstName,
            lastName,
            email,
            phoneNumber,
            description,
            academicTitle,
            institutions,
            countries,
            areas
        )
    {
        UserId = userId.ToString();
    }
}
