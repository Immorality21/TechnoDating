using MediatR;
using Microsoft.EntityFrameworkCore;
using TechnoDating.Api.Application.Photos.Requests;
using TechnoDating.Api.Application.Storage;
using TechnoDating.Api.Infrastructure;

namespace TechnoDating.Api.Application.Photos.Handlers;

public class DeletePhotoHandler(TechnoDatingDbContext db, IBlobStorage storage) : IRequestHandler<DeletePhotoRequest, bool>
{
    public async Task<bool> Handle(DeletePhotoRequest request, CancellationToken cancellationToken)
    {
        var photo = await db.Photos
            .FirstOrDefaultAsync(p => p.Id == request.PhotoId && p.UserId == request.UserId, cancellationToken);
        if (photo is null)
        {
            return false;
        }

        var user = await db.Users.FirstAsync(u => u.Id == request.UserId, cancellationToken);
        var wasPrimary = user.PrimaryPhotoId == photo.Id;
        var storageKey = photo.StorageKey;

        // Clear the FK explicitly before removing the photo so EF's change
        // tracker stays in sync with the SetNull cascade the DB will apply.
        if (wasPrimary)
        {
            user.PrimaryPhotoId = null;
        }

        db.Photos.Remove(photo);
        await db.SaveChangesAsync(cancellationToken);

        await storage.DeletePrefixAsync(storageKey, cancellationToken);

        if (wasPrimary)
        {
            var next = await db.Photos
                .Where(p => p.UserId == request.UserId)
                .OrderBy(p => p.Ordinal)
                .Select(p => (Guid?)p.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (next is not null)
            {
                user.PrimaryPhotoId = next;
                await db.SaveChangesAsync(cancellationToken);
            }
        }

        return true;
    }
}
