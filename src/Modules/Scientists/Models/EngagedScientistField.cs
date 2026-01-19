namespace Daab.Modules.Scientists.Models;

public class EngagedScientistField : BaseEntity
{
    public EngagedScientistField(string name)
    {
        Name = name;
    }

    public string Name { get; init; }
    public VectorOfStudy VectorOfStudy { get; set; }
}