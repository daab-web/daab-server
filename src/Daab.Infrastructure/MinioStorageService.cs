using Amazon.S3;
using Amazon.S3.Model;
using Daab.SharedKernel;

namespace Daab.Infrastructure;

public record MinioOptions(string AccessKey, string SecretKey, string Endpoint);

public class MinioStorageService(IAmazonS3 s3Client) : IBlobStorage
{
    public Task DeleteAsync(
        string containerName,
        string blobName,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }

    public async Task<Stream> DownloadAsync(
        string containerName,
        string blobName,
        CancellationToken cancellationToken = default
    )
    {
        var request = new GetObjectRequest { BucketName = containerName, Key = blobName };

        var response = await s3Client.GetObjectAsync(request, cancellationToken);

        return response.ResponseStream;
    }

    public Task<bool> ExistsAsync(
        string containerName,
        string blobName,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }

    public async Task UploadAsync(
        string containerName,
        string blobName,
        Stream data,
        CancellationToken cancellationToken = default
    )
    {
        var request = new PutObjectRequest
        {
            BucketName = containerName,
            Key = blobName,
            InputStream = data,
        };

        await s3Client.PutObjectAsync(request, cancellationToken);
    }
}
