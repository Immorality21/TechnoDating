namespace TechnoDating.Api.Application.Storage;

public interface IBlobStorage
{
    Task UploadAsync(string key, Stream content, string contentType, CancellationToken cancellationToken);
    Task DeleteAsync(string key, CancellationToken cancellationToken);
    Task DeletePrefixAsync(string keyPrefix, CancellationToken cancellationToken);
    string GetSignedUrl(string key);
    Task EnsureBucketExistsAsync(CancellationToken cancellationToken);
}
