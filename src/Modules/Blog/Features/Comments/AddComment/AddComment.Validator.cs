using FastEndpoints;
using FluentValidation;

namespace Daab.Modules.Blog.Features.Comments.AddComment;

public sealed class AddCommentRequestValidator : Validator<AddCommentRequest>
{
    public const int MaxLength = 2000;

    public AddCommentRequestValidator()
    {
        RuleFor(c => c.SourceLocale).NotEmpty();
        RuleFor(c => c.Texts).NotEmpty().WithMessage("Comment cannot be empty");
        RuleForEach(c => c.Texts)
            .Must(t => !string.IsNullOrWhiteSpace(t.Value))
            .WithMessage(t => "Comment cannot be empty")
            .Must(t => t.Value is null || t.Value.Length <= MaxLength)
            .WithMessage($"Comments can be up to {MaxLength} characters");
    }
}
