using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Daab.Modules.Activities.Features.News.UpdateNews;

public class UpdateNewsRequestValidator : Validator<UpdateNewsRequest>
{
    private const int MaxThumbnailSizeMb = 5;
    private const int MaxThumbnailSizeBytes = MaxThumbnailSizeMb * 1024 * 1024;

    public UpdateNewsRequestValidator()
    {
        RuleFor(x => x.Category)
            .MaximumLength(150)
            .When(x => !string.IsNullOrWhiteSpace(x.Category));

        RuleFor(x => x.Tags).NotNull();

        RuleForEach(x => x.Tags).NotEmpty().MaximumLength(100);
    }

    private static bool BeValidImage(IFormFile? file)
    {
        if (file is null)
            return true;
        if (file.Length is 0 or > MaxThumbnailSizeBytes)
            return false;

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        return allowedExtensions.Contains(extension);
    }
}
