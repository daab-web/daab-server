using Daab.Modules.Blog.Common;
using LanguageExt;
using MediatR;
using Unit = LanguageExt.Unit;

namespace Daab.Modules.Blog.Features.Posts.DeletePost;

public sealed record DeletePostCommand(BlogUser User, string PostId) : IRequest<Fin<Unit>>;
