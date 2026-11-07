using LanguageExt;
using MediatR;

namespace Daab.Modules.Blog.Features.Comments.GetComments;

public sealed record GetCommentsQuery(string PostId) : IRequest<Fin<List<CommentResponse>>>;
