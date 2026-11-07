namespace Daab.Modules.Blog.Features.Comments.TranslateComment;

public sealed record TranslateCommentResponse(
    string CommentId,
    string Locale,
    string Content,
    string? SourceLanguage
);
