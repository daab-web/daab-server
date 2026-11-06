using Daab.Modules.Blog.Common;
using LanguageExt;
using MediatR;

namespace Daab.Modules.Blog.Features.Comments.AddComment;

public sealed record AddCommentCommand(BlogUser Author, string PostId, string Content)
    : IRequest<Fin<CommentResponse>>;
