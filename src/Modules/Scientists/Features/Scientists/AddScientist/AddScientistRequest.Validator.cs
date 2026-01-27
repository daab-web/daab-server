namespace Daab.Modules.Scientists.Features.AddScientist;

using FluentValidation;

public sealed class AddScientistRequestValidator : AbstractValidator<AddScientistRequest>
{
    public AddScientistRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("Firt name is required");
        RuleFor(x => x.LastName).NotEmpty().WithMessage("Last name is required");

        RuleFor(x => x.Description).NotEmpty().WithMessage("Description is required");

        RuleFor(x => x.AcademicTitle).NotEmpty().WithMessage("Academic title is required");

        RuleFor(x => x.Institution).NotEmpty().WithMessage("Institution is required");

        RuleFor(x => x.Countries)
            .NotNull()
            .NotEmpty()
            .WithMessage("At least one country must be specified");

        RuleFor(x => x.Areas)
            .NotNull()
            .NotEmpty()
            .WithMessage("At least one area must be specified");
    }
}
