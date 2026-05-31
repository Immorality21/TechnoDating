using MediatR;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using TechnoDating.Api.Application.Photos.Requests;
using TechnoDating.Api.Application.Storage;
using TechnoDating.Api.Infrastructure;
using TechnoDating.Api.Infrastructure.Entities;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Photos.Handlers;

public class UploadPhotoHandler(TechnoDatingDbContext db, IBlobStorage storage, ILogger<UploadPhotoHandler> logger)
    : IRequestHandler<UploadPhotoRequest, PhotoDto?>
{
    private const int MaxPhotosPerUser = 6;
    private static readonly WebpEncoder Encoder = new() { Quality = 82 };

    public async Task<PhotoDto?> Handle(UploadPhotoRequest request, CancellationToken cancellationToken)
    {
        var existingCount = await db.Photos.CountAsync(p => p.UserId == request.UserId, cancellationToken);
        if (existingCount >= MaxPhotosPerUser)
        {
            logger.LogInformation("User {UserId} reached photo limit ({Limit})", request.UserId, MaxPhotosPerUser);
            return null;
        }

        Image image;
        try
        {
            image = await Image.LoadAsync(request.Content, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Photo upload: failed to decode image from user {UserId}", request.UserId);
            return null;
        }

        using (image)
        {
            var photoId = Guid.NewGuid();
            var storageKey = PhotoMappingExtensions.StorageKeyFor(request.UserId, photoId);
            var fullWidth = image.Width;
            var fullHeight = image.Height;

            await UploadVariantAsync(image, new ResizeOptions { Size = new Size(96, 96), Mode = ResizeMode.Crop, Position = AnchorPositionMode.Center },
                $"{storageKey}/{PhotoMappingExtensions.ThumbVariant}", cancellationToken);
            await UploadVariantAsync(image, new ResizeOptions { Size = new Size(480, 720), Mode = ResizeMode.Crop, Position = AnchorPositionMode.Center },
                $"{storageKey}/{PhotoMappingExtensions.CardVariant}", cancellationToken);
            await UploadVariantAsync(image, new ResizeOptions { Size = new Size(1080, 1620), Mode = ResizeMode.Max },
                $"{storageKey}/{PhotoMappingExtensions.FullVariant}", cancellationToken);

            var nextOrdinal = existingCount == 0
                ? 0
                : await db.Photos.Where(p => p.UserId == request.UserId).MaxAsync(p => (int?)p.Ordinal, cancellationToken) + 1 ?? 0;
            var becomesPrimary = existingCount == 0;

            var photo = new Photo
            {
                Id = photoId,
                UserId = request.UserId,
                Ordinal = nextOrdinal,
                IsPrimary = becomesPrimary,
                Width = fullWidth,
                Height = fullHeight,
                StorageKey = storageKey,
                ContentType = "image/webp",
                ModerationStatus = "approved",
                UploadedAt = DateTimeOffset.UtcNow,
            };

            db.Photos.Add(photo);
            await db.SaveChangesAsync(cancellationToken);

            return photo.ToDto(storage);
        }
    }

    private async Task UploadVariantAsync(Image source, ResizeOptions options, string key, CancellationToken cancellationToken)
    {
        using var resized = source.Clone(ctx => ctx.Resize(options));
        using var stream = new MemoryStream();
        await resized.SaveAsync(stream, Encoder, cancellationToken);
        stream.Position = 0;
        await storage.UploadAsync(key, stream, "image/webp", cancellationToken);
    }
}
