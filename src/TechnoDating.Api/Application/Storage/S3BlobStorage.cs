using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.Extensions.Options;

namespace TechnoDating.Api.Application.Storage;

public class S3BlobStorage(IAmazonS3 s3, IOptions<StorageOptions> options, ILogger<S3BlobStorage> logger) : IBlobStorage
{
    private readonly StorageOptions _opts = options.Value;

    public async Task UploadAsync(string key, Stream content, string contentType, CancellationToken cancellationToken)
    {
        await s3.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _opts.Bucket,
            Key = key,
            InputStream = content,
            ContentType = contentType,
            AutoCloseStream = false,
        }, cancellationToken);
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken)
    {
        await s3.DeleteObjectAsync(_opts.Bucket, key, cancellationToken);
    }

    public async Task DeletePrefixAsync(string keyPrefix, CancellationToken cancellationToken)
    {
        var listed = await s3.ListObjectsV2Async(new ListObjectsV2Request
        {
            BucketName = _opts.Bucket,
            Prefix = keyPrefix,
        }, cancellationToken);

        if (listed.S3Objects is null || listed.S3Objects.Count == 0)
        {
            return;
        }

        await s3.DeleteObjectsAsync(new DeleteObjectsRequest
        {
            BucketName = _opts.Bucket,
            Objects = listed.S3Objects.Select(o => new KeyVersion { Key = o.Key }).ToList(),
        }, cancellationToken);
    }

    public string GetSignedUrl(string key)
    {
        var useHttp = _opts.Endpoint.StartsWith("http://", StringComparison.OrdinalIgnoreCase);
        return s3.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = _opts.Bucket,
            Key = key,
            Expires = DateTime.UtcNow.AddMinutes(_opts.SignedUrlMinutes),
            Verb = HttpVerb.GET,
            Protocol = useHttp ? Protocol.HTTP : Protocol.HTTPS,
        });
    }

    public async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var exists = await AmazonS3Util.DoesS3BucketExistV2Async(s3, _opts.Bucket);
            if (exists)
            {
                return;
            }
            await s3.PutBucketAsync(new PutBucketRequest { BucketName = _opts.Bucket }, cancellationToken);
            logger.LogInformation("Created blob bucket {Bucket}", _opts.Bucket);
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "BucketAlreadyOwnedByYou" || ex.ErrorCode == "BucketAlreadyExists")
        {
            // Concurrent startup race — fine.
        }
    }
}
