using System.ComponentModel.DataAnnotations;

namespace Daab.Modules.Scientists.Models;

public class Scientist
{
    [MaxLength(36)]
    public string Id { get; init; } = Ulid.NewUlid().ToString();

    [MaxLength(36)]
    public string? UserId { get; private set; }

    [MaxLength(320)]
    public string? Email { get; set; }

    [MaxLength(15)]
    public string? PhoneNumber { get; set; }

    [MaxLength(64)]
    public string FirstName { get; set; }

    [MaxLength(64)]
    public string LastName { get; set; }

    public string? Description { get; set; }

    [MaxLength(320)]
    public string AcademicTitle { get; set; }

    public required string Slug { get; set; }
    public string? PhotoUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? Orcid { get; set; }
    public string? Website { get; set; }

    public ICollection<string> Institutions { get; set; }
    public ICollection<string> Countries { get; set; }
    public ICollection<string> Areas { get; set; }
    public ICollection<Publication> Publications { get; set; } = [];

    public Scientist(
        string firstName,
        string lastName,
        string? email,
        string? phoneNumber,
        string? description,
        string academicTitle,
        ICollection<string> institutions,
        ICollection<string> countries,
        ICollection<string> areas,
        string? photoUrl,
        string? linkedInUrl,
        string? orcid,
        string? website
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
        PhotoUrl = photoUrl;
        LinkedInUrl = linkedInUrl;
        Orcid = orcid;
        Website = website;
    }

    public void LinkUser(string userId)
    {
        UserId = Ulid.TryParse(userId, out _)
            ? userId
            : throw new ArgumentException("Invalid ID format");
    }
}
