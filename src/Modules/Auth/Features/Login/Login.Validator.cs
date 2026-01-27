using FluentValidation;

namespace Daab.Modules.Auth.Features.Login;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(u => u.Username).NotEmpty().WithMessage("Username is required");
        RuleFor(u => u.Password).NotEmpty().WithMessage("Username is required");
    }
}
