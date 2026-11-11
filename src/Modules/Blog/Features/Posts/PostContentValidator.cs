using Daab.Modules.Blog.Common;
using Daab.Modules.Blog.Models;
using Daab.SharedKernel.Middlewares;
using FastEndpoints;
using FluentValidation;

namespace Daab.Modules.Blog.Features.Posts;

/// <summary>A post's content in one language, as sent by the editor.</summary>
public sealed record PostTranslationInput(
    string Locale,
    string Title,
    string Excerpt,
    string EditorState
);

/// <summary>The editable content of a post, shared by create and update.</summary>
public interface IPostContent : ILocalizedCollection
{
    string SourceLocale { get; }
    List<PostTranslationInput> Translations { get; }

    IEnumerable<string> ILocalizedCollection.GetLocales() =>
        Translations.Select(t => t.Locale).Append(SourceLocale);

    IReadOnlyCollection<PostContent> ToContents() =>
        [
            .. Translations.Select(t => new PostContent(
                t.Locale,
                t.Title,
                t.Excerpt ?? string.Empty,
                t.EditorState
            )),
        ];
}

public abstract class PostContentValidator<T> : Validator<T>
    where T : IPostContent
{
    protected PostContentValidator()
    {
        RuleFor(p => p.SourceLocale).NotEmpty();
        RuleFor(p => p.Translations)
            .NotEmpty()
            .WithMessage("Provide the post in every language")
            .Must(t => t.Select(x => x.Locale).Distinct().Count() == t.Count)
            .WithMessage("Each language may appear only once");

        RuleForEach(p => p.Translations)
            .ChildRules(t =>
            {
                t.RuleFor(x => x.Locale).NotEmpty();
                t.RuleFor(x => x.Title)
                    .NotEmpty()
                    .WithMessage(x => $"Title is required ({x.Locale})")
                    .MaximumLength(200);
                t.RuleFor(x => x.Excerpt).MaximumLength(500);
                t.RuleFor(x => x.EditorState)
                    .NotEmpty()
                    .WithMessage(x => $"Content is required ({x.Locale})")
                    .MaximumLength(1_000_000)
                    .Must(LexicalText.IsValid)
                    .WithMessage(x => $"Content is not a valid editor state ({x.Locale})");
            });
    }
}
