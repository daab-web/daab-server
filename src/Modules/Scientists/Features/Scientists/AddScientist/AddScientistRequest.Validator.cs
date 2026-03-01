using FastEndpoints;
using FluentValidation;

namespace Daab.Modules.Scientists.Features.Scientists.AddScientist;

public sealed class AddScientistRequestValidator : Validator<AddScientistRequest>
{
    public AddScientistRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name is required");
        RuleFor(x => x.LastName).NotEmpty().WithMessage("Last name is required");

        RuleFor(x => x.Description).NotEmpty().WithMessage("Description is required");

        RuleFor(x => x.AcademicTitle).NotEmpty().WithMessage("Academic title is required");

        RuleFor(x => x.Institution)
            .NotNull()
            .NotEmpty()
            .WithMessage("At least one institution must be specified");

        RuleForEach(x => x.Institution).NotEmpty().WithMessage("Institution cannot be empty");

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
