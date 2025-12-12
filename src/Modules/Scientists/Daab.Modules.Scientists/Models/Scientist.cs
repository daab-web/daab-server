namespace Daab.Modules.Scientists.Models;

public class Scientist
{
    public string Id { get; private set; }
    public string? UserId { get; private set; }

    public string Description { get; private set; }
    public string AcademicTitle { get; private set; }
    public string Institution { get; private set; }

    public List<string> Countries { get; private set; }
    public List<string> Areas { get; private set; }

    public Scientist(
        string description,
        string academicTitle,
        string institution,
        List<string> countries,
        List<string> areas
    )
    {
        Id = Guid.NewGuid().ToString();
        Description = description;
        AcademicTitle = academicTitle;
        Institution = institution;
        Countries = countries;
        Areas = areas;
    }

    public Scientist(
        Guid userId,
        string description,
        string academicTitle,
        string institution,
        List<string> countries,
        List<string> areas
    ) : this(description, academicTitle, institution, countries, areas)
    {
        UserId = userId.ToString();
    }
}

