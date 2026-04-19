using System.Diagnostics.CodeAnalysis;
using LanguageExt;
using MediatR;
using Microsoft.AspNetCore.Http;

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
        Institution = request.Institutions;
        Countries = request.Countries;
        Areas = request.Areas;
        LinkedInUrl = request.LinkedInUrl;
        Orcid = request.Orcid;
        Website = request.Website;
    }

    public string Id { get; }
    public string? Email { get; }
    public string? PhoneNumber { get; }
    public string AcademicTitle { get; }
    public string? LinkedInUrl { get; set; }
    public string? Orcid { get; set; }
    public string? Website { get; set; }
    public string[] Institution { get; }
    public string[] Countries { get; }
    public string[] Areas { get; }
}
