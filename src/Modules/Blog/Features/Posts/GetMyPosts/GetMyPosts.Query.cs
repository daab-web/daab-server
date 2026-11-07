using LanguageExt;
using MediatR;

namespace Daab.Modules.Blog.Features.Posts.GetMyPosts;

public sealed record GetMyPostsQuery(string AuthorId) : IRequest<Fin<List<MyPostResponse>>>;
