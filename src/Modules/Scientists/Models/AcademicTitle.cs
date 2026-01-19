namespace Daab.Modules.Scientists.Models;

public class AcademicTitle : BaseEntity
{
    public AcademicTitle(string title, string description)
    {
        Title = title;
        Description = description;
    }

    public string Title { get; }
    public string Description { get; }
}