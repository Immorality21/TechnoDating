using TechnoDating.Api.Application.Storage;

namespace TechnoDating.Api.Infrastructure.Seeding;

public class BlobStorageInitializer(IServiceProvider services, ILogger<BlobStorageInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var storage = scope.ServiceProvider.GetRequiredService<IBlobStorage>();

        logger.LogInformation("Ensuring blob bucket exists...");
        try
        {
            await storage.EnsureBucketExistsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Blob bucket bootstrap failed");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
