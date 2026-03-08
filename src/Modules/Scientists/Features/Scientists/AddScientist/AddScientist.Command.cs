using Daab.Modules.Scientists.Models;
using MediatR;

namespace Daab.Modules.Scientists.Features.Scientists.AddScientist;

public record AddScientistCommand(Scientist Scientist) : IRequest<Scientist>;
