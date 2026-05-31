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

        var wasPrimary = photo.IsPrimary;
        var storageKey = photo.StorageKey;

        db.Photos.Remove(photo);
        await db.SaveChangesAsync(cancellationToken);

        await storage.DeletePrefixAsync(storageKey, cancellationToken);

        if (wasPrimary)
        {
            var next = await db.Photos
                .Where(p => p.UserId == request.UserId)
                .OrderBy(p => p.Ordinal)
                .FirstOrDefaultAsync(cancellationToken);
            if (next is not null)
            {
                next.IsPrimary = true;
                await db.SaveChangesAsync(cancellationToken);
            }
        }

        return true;
    }
}
