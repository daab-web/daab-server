using FastEndpoints;
using FluentValidation;

namespace Daab.Modules.Scientists.Features.Scientists.UpdateScientist;

public class UpdateScientistRequestValidator : Validator<UpdateScientistRequest>
{
    public UpdateScientistRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email can't be null")
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(32)
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);

        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.AcademicTitle).NotEmpty().MaximumLength(150);

        RuleFor(x => x.Institution).NotEmpty().MaximumLength(200);

        RuleFor(x => x.Countries)
            .NotNull()
            .Must(c => c.Any())
            .WithMessage("At least one country must be specified.");

        RuleForEach(x => x.Countries).NotEmpty().MaximumLength(100);

        RuleFor(x => x.Areas)
            .NotNull()
            .Must(a => a.Any())
            .WithMessage("At least one area must be specified.");

        RuleForEach(x => x.Areas).NotEmpty().MaximumLength(100);
    }
}
