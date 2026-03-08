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
        Institution = request.Institutions;
        PhoneNumber = request.PhoneNumber;
        LastName = request.LastName;
        Institution = request.Institutions;
        FirstName = request.FirstName;
        Description = request.Description;
        Countries = request.Countries;
        Areas = request.Areas;
    }

    public string Id { get; }
    public string? Email { get; }
    public string? PhoneNumber { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string? Description { get; }
    public string AcademicTitle { get; }
    public string[] Institution { get; }
    public string[] Countries { get; }
    public string[] Areas { get; }
}
