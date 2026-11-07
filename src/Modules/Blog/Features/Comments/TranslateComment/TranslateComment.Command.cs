using LanguageExt;
using MediatR;

namespace Daab.Modules.Blog.Features.Comments.TranslateComment;

public sealed record TranslateCommentCommand(string CommentId, string Locale)
    : IRequest<Fin<TranslateCommentResponse>>;
