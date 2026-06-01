using TechnoDating.Api.Application.Storage;

namespace TechnoDating.Api.Tests;

/// <summary>No-op blob storage for handler tests that don't exercise real photo I/O.</summary>
internal sealed class FakeBlobStorage : IBlobStorage
{
    public Task UploadAsync(string key, Stream content, string contentType, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task DeletePrefixAsync(string keyPrefix, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public string GetSignedUrl(string key)
    {
        return $"https://test.local/{key}";
    }

    public Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
