using Microsoft.EntityFrameworkCore;
using TechnoDating.Api.Application.Storage;
using TechnoDating.Api.Infrastructure;
using TechnoDating.Api.Infrastructure.Entities;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Photos;

public static class PhotoMappingExtensions
{
    public const string ThumbVariant = "thumb.webp";
    public const string CardVariant = "card.webp";
    public const string FullVariant = "full.webp";

    public static PhotoDto ToDto(this Photo photo, IBlobStorage storage, bool isPrimary)
    {
        return new PhotoDto(
            photo.Id,
            photo.Ordinal,
            isPrimary,
            ThumbUrl: storage.GetSignedUrl($"{photo.StorageKey}/{ThumbVariant}"),
            CardUrl: storage.GetSignedUrl($"{photo.StorageKey}/{CardVariant}"),
            FullUrl: storage.GetSignedUrl($"{photo.StorageKey}/{FullVariant}"));
    }

    public static string StorageKeyFor(Guid userId, Guid photoId)
    {
        return $"users/{userId}/photos/{photoId}";
    }

    public static async Task<IReadOnlyList<PhotoDto>> LoadPhotosAsync(
        this TechnoDatingDbContext db,
        IBlobStorage storage,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var primaryPhotoId = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.PrimaryPhotoId)
            .FirstOrDefaultAsync(cancellationToken);

        var rows = await db.Photos
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.Ordinal)
            .ToListAsync(cancellationToken);
        return rows.Select(p => p.ToDto(storage, isPrimary: primaryPhotoId == p.Id)).ToList();
    }

    public static async Task<Dictionary<Guid, string>> LoadPrimaryPhotoCardUrlsAsync(
        this TechnoDatingDbContext db,
        IBlobStorage storage,
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken)
    {
        if (userIds.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }
        var primaries = await db.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id) && u.PrimaryPhotoId != null)
            .Join(
                db.Photos.AsNoTracking(),
                u => u.PrimaryPhotoId,
                p => p.Id,
                (u, p) => new { UserId = u.Id, p.StorageKey })
            .ToListAsync(cancellationToken);
        return primaries.ToDictionary(
            x => x.UserId,
            x => storage.GetSignedUrl($"{x.StorageKey}/{CardVariant}"));
    }
}
