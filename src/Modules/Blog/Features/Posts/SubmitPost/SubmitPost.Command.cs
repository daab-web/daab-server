using Daab.Modules.Blog.Common;
using LanguageExt;
using MediatR;

namespace Daab.Modules.Blog.Features.Posts.SubmitPost;

/// <summary>Runs content verification and publishes the post if it passes.</summary>
public sealed record SubmitPostCommand(BlogUser Author, string PostId)
    : IRequest<Fin<SubmitPostResponse>>;
