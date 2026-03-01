using MediatR;

namespace Daab.Modules.Scientists.Features.Scientists.AddScientist;

public record AddScientistCommand : IRequest<AddScientistResponse>
{
    public string? UserId { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string Email { get; }
    public string? PhoneNumber { get; }
    public string Description { get; }
    public string AcademicTitle { get; }
    public string[] Institution { get; }
    public string[] Countries { get; }
    public string[] Areas { get; }

    public AddScientistCommand(AddScientistRequest request)
    {
        UserId = request.UserId;
        FirstName = request.FirstName;
        LastName = request.LastName;
        Email = request.Email;
        PhoneNumber = request.PhoneNumber;
        Description = request.Description;
        AcademicTitle = request.AcademicTitle;
        Institution = request.Institution;
        Countries = request.Countries;
        Areas = request.Areas;
    }
}
