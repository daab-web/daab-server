namespace Daab.Modules.Activities.Messages;

public sealed record ThumbnailUploadMessage(string NewsId, byte[] ImageData);
