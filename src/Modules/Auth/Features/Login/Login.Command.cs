using LanguageExt;
using MediatR;

namespace Daab.Modules.Auth.Features.Login;

public sealed record LoginCommand : IRequest<Fin<(LoginResponse, Models.RefreshToken refreshToken)>>
{
    public string Username { get; }
    public string Password { get; }

    public LoginCommand(LoginRequest request)
    {
        Username = request.Username;
        Password = request.Password;
    }
}
