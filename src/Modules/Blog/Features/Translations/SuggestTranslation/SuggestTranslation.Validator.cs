using FastEndpoints;
using FluentValidation;

namespace Daab.Modules.Blog.Features.Translations.SuggestTranslation;

public sealed class SuggestTranslationRequestValidator : Validator<SuggestTranslationRequest>
{
    public SuggestTranslationRequestValidator()
    {
        RuleFor(r => r.TargetLocale)
            .NotEqual(r => r.SourceLocale)
            .WithMessage("Choose a different target language");
        RuleFor(r => r.Texts)
            .NotEmpty()
            .Must(t => t.Count <= 5)
            .WithMessage("Translate at most 5 texts at once")
            .Must(t => t.Sum(x => x?.Length ?? 0) <= 6000)
            .WithMessage("The texts are too long to translate at once");
        RuleForEach(r => r.Texts).NotNull().MaximumLength(2000);
    }
}
