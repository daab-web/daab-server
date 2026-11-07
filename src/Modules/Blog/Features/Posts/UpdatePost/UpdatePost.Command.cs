using Daab.Modules.Blog.Common;
using LanguageExt;
using MediatR;

namespace Daab.Modules.Blog.Features.Posts.UpdatePost;

public sealed record UpdatePostCommand(BlogUser Author, UpdatePostRequest Content)
    : IRequest<Fin<PostResponse>>;
