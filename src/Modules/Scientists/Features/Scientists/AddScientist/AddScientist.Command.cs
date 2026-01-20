using MediatR;

namespace Daab.Modules.Scientists.Features.AddScientist;

public record AddScientistCommand : IRequest<AddScientistResponse>
{
    public string? UserId { get; }
    public string FullName { get; }
    public string Description { get; }
    public string AcademicTitle { get; }
    public string Institution { get; }
    public string[] Countries { get; }
    public string[] Areas { get; }

    public AddScientistCommand(AddScientistRequest request)
    {
        UserId = request.UserId;
        FullName = request.FullName;
        Description = request.Description;
        AcademicTitle = request.AcademicTitle;
        Institution = request.Institution;
        Countries = request.Countries;
        Areas = request.Areas;
    }
}
