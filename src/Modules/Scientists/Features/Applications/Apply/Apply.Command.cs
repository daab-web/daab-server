using LanguageExt;
using MediatR;

namespace Daab.Modules.Scientists.Features.Applications.Apply;

public sealed record ApplyCommand(ApplyRequest Data) : IRequest<Fin<ApplyResponse>>;
