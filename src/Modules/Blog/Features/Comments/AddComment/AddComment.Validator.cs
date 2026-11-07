using FastEndpoints;
using FluentValidation;

namespace Daab.Modules.Blog.Features.Comments.AddComment;

public sealed class AddCommentRequestValidator : Validator<AddCommentRequest>
{
    public AddCommentRequestValidator()
    {
        RuleFor(c => c.Content)
            .Must(c => !string.IsNullOrWhiteSpace(c))
            .WithMessage("Comment cannot be empty")
            .MaximumLength(2000);
    }
}
