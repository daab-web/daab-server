using Daab.Modules.Scientists.Models;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Daab.Modules.Scientists.Features.Scientists.AddScientist;

public record AddScientistCommand(Scientist Scientist, IFormFile? Photo) : IRequest<Scientist>;
