using FastEndpoints;
using FluentValidation;

namespace Daab.Modules.Auth.Features.Register;

public sealed class RegisterRequestValidator : Validator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(r => r.FullName).NotEmpty().WithMessage("Full name is required").MaximumLength(120);

        RuleFor(r => r.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email is not valid")
            .MaximumLength(320);

        RuleFor(r => r.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long")
            .MaximumLength(128)
            .Matches("[A-Za-z]")
            .WithMessage("Password must contain a letter")
            .Matches("[0-9]")
            .WithMessage("Password must contain a digit");
    }
}
