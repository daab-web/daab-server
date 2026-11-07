using Daab.Modules.Blog.Common;
using LanguageExt;
using MediatR;
using Unit = LanguageExt.Unit;

namespace Daab.Modules.Blog.Features.Comments.DeleteComment;

public sealed record DeleteCommentCommand(BlogUser User, string CommentId) : IRequest<Fin<Unit>>;
