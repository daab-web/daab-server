using LanguageExt;
using MediatR;

namespace Daab.Modules.Auth.Features.Register;

public sealed record RegisterCommand(string FullName, string Email, string Password)
    : IRequest<Fin<RegisterResponse>>
{
    public RegisterCommand(RegisterRequest request)
        : this(request.FullName, request.Email, request.Password) { }
}
