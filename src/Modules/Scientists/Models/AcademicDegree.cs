namespace Daab.Modules.Scientists.Models;

public class AcademicDegree : BaseEntity
{
    public AcademicDegree(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public string Name { get; }
    public string Description { get; }
}