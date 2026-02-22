using System.Diagnostics.CodeAnalysis;
using LanguageExt;
using MediatR;

namespace Daab.Modules.Scientists.Features.Scientists.UpdateScientist;

public sealed class UpdateScientistCommand : IRequest<Fin<UpdateScientistResponse>>
{
    public UpdateScientistCommand(string id, UpdateScientistRequest request)
    {
        Id = id;
        Email = request.Email;
        AcademicTitle = request.AcademicTitle;
        Institution = request.Institution;
        PhoneNumber = request.PhoneNumber;
        LastName = request.LastName;
        Institution = request.Institution;
        FirstName = request.FirstName;
        Description = request.Description;
        Countries = request.Countries;
        Areas = request.Areas;
    }

    public string Id { get; set; }
    public string Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string? Description { get; private set; }
    public string AcademicTitle { get; private set; }
    public string Institution { get; private set; }
    public IEnumerable<string> Countries { get; set; }
    public IEnumerable<string> Areas { get; set; }
}
