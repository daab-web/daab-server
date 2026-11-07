using Daab.Modules.Blog.Common;
using FastEndpoints;
using FluentValidation;

namespace Daab.Modules.Blog.Features.Posts;

/// <summary>The editable content of a post, shared by create and update.</summary>
public interface IPostContent
{
    string Title { get; }
    string Excerpt { get; }
    string EditorState { get; }
    string Locale { get; }
}

public abstract class PostContentValidator<T> : Validator<T>
    where T : IPostContent
{
    protected PostContentValidator()
    {
        RuleFor(p => p.Title).NotEmpty().WithMessage("Title is required").MaximumLength(200);
        RuleFor(p => p.Excerpt).NotNull().MaximumLength(500);
        RuleFor(p => p.Locale).NotEmpty();
        RuleFor(p => p.EditorState)
            .NotEmpty()
            .WithMessage("Content is required")
            .MaximumLength(1_000_000)
            .Must(LexicalText.IsValid)
            .WithMessage("Content is not a valid editor state");
    }
}
