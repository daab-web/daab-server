using Daab.Modules.Blog.Models;
using Daab.SharedKernel;
using LanguageExt;
using MediatR;

namespace Daab.Modules.Blog.Features.Posts.GetAllPosts;

/// <summary>Every post in any status — the admin's moderation list.</summary>
public sealed record GetAllPostsQuery(int Page, int PageSize, PostStatus? Status)
    : IRequest<Fin<PagedResponse<AdminPostResponse>>>;

/// <param name="Title">The title in the original language.</param>
public sealed record AdminPostResponse(
    string Id,
    string Slug,
    string Title,
    string SourceLocale,
    IReadOnlyList<string> Locales,
    string AuthorId,
    string AuthorName,
    PostStatus Status,
    string? VerificationFeedback,
    DateTime UpdatedAt,
    DateTime? PublishedAt,
    int CommentCount
);
