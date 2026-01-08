namespace Daab.Modules.Scientists.Models;

public class Scientist
{
    public string Id { get; private set; }
    public string? UserId { get; private set; }

    public string FullName { get; private set; }
    public string Description { get; private set; }
    public string AcademicTitle { get; private set; }
    public string Institution { get; private set; }

    public IEnumerable<string> Countries { get; private set; }
    public IEnumerable<string> Areas { get; private set; }

    public string Slug()
    {
        return $"{FullName.Replace(' ', '-')}-{Id[..5]}";
    }

    public Scientist(
        string fullName,
        string description,
        string academicTitle,
        string institution,
        IEnumerable<string> countries,
        IEnumerable<string> areas
    )
    {
        Id = Guid.NewGuid().ToString();
        FullName = fullName;
        Description = description;
        AcademicTitle = academicTitle;
        Institution = institution;
        Countries = countries;
        Areas = areas;
    }

    public Scientist(
        Guid userId,
        string fullName,
        string description,
        string academicTitle,
        string institution,
        IEnumerable<string> countries,
        IEnumerable<string> areas
    )
        : this(fullName, description, academicTitle, institution, countries, areas)
    {
        UserId = userId.ToString();
    }
}
