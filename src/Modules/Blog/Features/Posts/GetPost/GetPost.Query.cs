using Daab.Modules.Blog.Common;
using LanguageExt;
using MediatR;

namespace Daab.Modules.Blog.Features.Posts.GetPost;

public sealed record GetPostQuery(string IdOrSlug, BlogUser? Viewer) : IRequest<Fin<PostResponse>>;
