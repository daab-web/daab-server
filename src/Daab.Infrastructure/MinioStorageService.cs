using Amazon.S3;
using Amazon.S3.Model;
using Daab.SharedKernel;

namespace Daab.Infrastructure;

public record MinioOptions(string AccessKey, string SecretKey, string Endpoint);

public class MinioStorageService(IAmazonS3 s3Client) : IBlobStorage
{
    public async Task DeleteAsync(
        string containerName,
        string blobName,
        CancellationToken cancellationToken = default
    )
    {
        await s3Client.DeleteObjectAsync(containerName, blobName, cancellationToken);
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

    public async Task<bool> ExistsAsync(
        string containerName,
        string blobName,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await s3Client.GetObjectMetadataAsync(containerName, blobName, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task<int> UploadAsync(
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

        var response = await s3Client.PutObjectAsync(request, cancellationToken);
        return (int)response.HttpStatusCode;
    }
}
