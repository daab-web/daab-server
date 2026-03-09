namespace Daab.Modules.Scientists.Messages;

public sealed record ProfilePictureUploadMessage(string ScientistId, byte[] ImageData);
