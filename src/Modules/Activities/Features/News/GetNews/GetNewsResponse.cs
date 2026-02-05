namespace Daab.Modules.Activities.Features.News.GetNews;

public sealed record GetNewsResponse(string Id, string Title, string Thumbnail, object? EditorState);