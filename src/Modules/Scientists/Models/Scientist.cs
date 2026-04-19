using System.ComponentModel.DataAnnotations;
using Daab.SharedKernel.Entities;

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

    [MaxLength(320)]
    public string AcademicTitle { get; set; }

    public required string Slug { get; set; }
    public string? PhotoUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? Orcid { get; set; }
    public string? Website { get; set; }

    public ICollection<ScientistTranslation> Translations { get; set; } = [];

    public DateTime? DateOfBirth
    {
        get;
        set
        {
            if (value > DateTime.UtcNow)
            {
                throw new ArgumentException("Date of birth must not be in the future");
            }

            field = value;
        }
    }

    public ICollection<string> Institutions { get; set; }
    public ICollection<string> Countries { get; set; }
    public ICollection<string> Areas { get; set; }
    public ICollection<Publication> Publications { get; set; } = [];

    public Scientist(
        string? email,
        string? phoneNumber,
        string academicTitle,
        ICollection<string> institutions,
        ICollection<string> countries,
        ICollection<string> areas,
        string? photoUrl,
        string? linkedInUrl,
        string? orcid,
        string? website,
        DateTime? dateOfBirth
    )
    {
        Email = email;
        PhoneNumber = phoneNumber;
        AcademicTitle = academicTitle;
        Institutions = institutions;
        Countries = countries;
        Areas = areas;
        PhotoUrl = photoUrl;
        LinkedInUrl = linkedInUrl;
        Orcid = orcid;
        Website = website;
        DateOfBirth = dateOfBirth;
    }

    public void LinkUser(string userId)
    {
        UserId = Ulid.TryParse(userId, out _)
            ? userId
            : throw new ArgumentException("Invalid ID format");
    }
}

public class ScientistTranslation
{
    public required string Locale { get; init; }
    public required string ScientistId { get; set; }
    public Scientist Scientist { get; set; } = null!;

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Description { get; set; }

    public TranslationStatus Status { get; private set; } = TranslationStatus.Untranslated;

    public void Update(string firstName, string lastName, string description)
    {
        FirstName = firstName;
        LastName = lastName;
        Description = description;
        Status = TranslationStatus.Translated;
    }
}
