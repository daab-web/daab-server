using FastEndpoints;
using FluentValidation;

namespace Daab.Modules.Auth.Features.Login;

public class LoginRequestValidator : Validator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(u => u.Username).NotEmpty().WithMessage("Username is required");
        RuleFor(u => u.Password).NotEmpty().WithMessage("Username is required");
    }
}
