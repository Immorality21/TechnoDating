using MediatR;
using Microsoft.EntityFrameworkCore;
using TechnoDating.Api.Application.Photos.Requests;
using TechnoDating.Api.Application.Storage;
using TechnoDating.Api.Infrastructure;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Photos.Handlers;

public class SetPrimaryPhotoHandler(TechnoDatingDbContext db, IBlobStorage storage) : IRequestHandler<SetPrimaryPhotoRequest, PhotoDto?>
{
    public async Task<PhotoDto?> Handle(SetPrimaryPhotoRequest request, CancellationToken cancellationToken)
    {
        var target = await db.Photos
            .FirstOrDefaultAsync(p => p.Id == request.PhotoId && p.UserId == request.UserId, cancellationToken);
        if (target is null)
        {
            return null;
        }

        if (target.IsPrimary)
        {
            return target.ToDto(storage);
        }

        // Clear current primary(ies) before setting the new one to keep the
        // filtered unique index happy (WHERE IsPrimary = true).
        var current = await db.Photos
            .Where(p => p.UserId == request.UserId && p.IsPrimary && p.Id != target.Id)
            .ToListAsync(cancellationToken);
        foreach (var p in current)
        {
            p.IsPrimary = false;
        }
        await db.SaveChangesAsync(cancellationToken);

        target.IsPrimary = true;
        await db.SaveChangesAsync(cancellationToken);

        return target.ToDto(storage);
    }
}
