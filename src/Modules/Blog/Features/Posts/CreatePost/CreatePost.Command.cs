using Daab.Modules.Blog.Common;
using LanguageExt;
using MediatR;

namespace Daab.Modules.Blog.Features.Posts.CreatePost;

public sealed record CreatePostCommand(BlogUser Author, CreatePostRequest Content)
    : IRequest<Fin<CreatePostResponse>>;
