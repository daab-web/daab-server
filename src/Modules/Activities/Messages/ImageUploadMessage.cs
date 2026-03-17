namespace Daab.Modules.Activities.Messages;

public enum MessageType
{
    Thumbnail,
    Attachment,
}

public sealed record UploadMessage(
    string ActivityId,
    string Name,
    byte[] ImageData,
    MessageType Type
);
