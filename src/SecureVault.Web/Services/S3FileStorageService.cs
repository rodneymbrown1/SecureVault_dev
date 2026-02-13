using Amazon.S3;
using Amazon.S3.Model;

namespace SecureVault.Web.Services;

public class S3FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly ILogger<S3FileStorageService> _logger;

    public S3FileStorageService(IAmazonS3 s3Client, IConfiguration configuration, ILogger<S3FileStorageService> logger)
    {
        _s3Client = s3Client;
        _bucketName = configuration["AWS_S3_BUCKET"] ?? configuration["AWS:S3Bucket"] ?? "securevault-files";
        _logger = logger;
    }

    public async Task<string> UploadAsync(string key, Stream data, CancellationToken cancellationToken = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = data,
            ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
        };

        await _s3Client.PutObjectAsync(request, cancellationToken);
        _logger.LogInformation("Uploaded file to S3: {Key}", key);
        return key;
    }

    public async Task<Stream> DownloadAsync(string key, CancellationToken cancellationToken = default)
    {
        var request = new GetObjectRequest
        {
            BucketName = _bucketName,
            Key = key
        };

        var response = await _s3Client.GetObjectAsync(request, cancellationToken);

        // Copy to memory stream so the S3 response can be disposed
        var ms = new MemoryStream();
        await response.ResponseStream.CopyToAsync(ms, cancellationToken);
        ms.Position = 0;
        return ms;
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = key
        };

        await _s3Client.DeleteObjectAsync(request, cancellationToken);
        _logger.LogInformation("Deleted file from S3: {Key}", key);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = _bucketName,
                Key = key
            };
            await _s3Client.GetObjectMetadataAsync(request, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }
}
