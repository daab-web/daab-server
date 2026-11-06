using Daab.SharedKernel;
using LanguageExt;
using MediatR;

namespace Daab.Modules.Blog.Features.Posts.GetPublishedPosts;

public sealed record GetPublishedPostsQuery(int Page, int PageSize, string? ScientistId)
    : IRequest<Fin<PagedResponse<PostSummaryResponse>>>;
