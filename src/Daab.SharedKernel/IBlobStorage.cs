namespace Daab.SharedKernel;

public interface IBlobStorage
{
    Task<int> UploadAsync(
        string containerName,
        string blobName,
        Stream data,
        CancellationToken cancellationToken
    );

    Task<Stream> DownloadAsync(
        string containerName,
        string blobName,
        CancellationToken cancellationToken
    );

    Task DeleteAsync(string containerName, string blobName, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(
        string containerName,
        string blobName,
        CancellationToken cancellationToken
    );
}
