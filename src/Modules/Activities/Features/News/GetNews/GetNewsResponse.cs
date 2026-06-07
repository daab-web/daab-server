namespace Daab.Modules.Activities.Features.News.GetNews;

public sealed record GetNewsResponse(
    string Id,
    string Title,
    string Slug,
    string? Thumbnail,
    string? Excerpt,
    string PublishedDate,
    string? AuthorId,
    string? AuthorName,
    string? Category,
    List<string> Tags,
    string? EditorState
);

public sealed record AttachmentDto(
    string Id,
    string FileUrl,
    string? Caption,
    string? FileType,
    string ParentObjectId
);
