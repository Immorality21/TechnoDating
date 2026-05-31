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
        var photo = await db.Photos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.PhotoId && p.UserId == request.UserId, cancellationToken);
        if (photo is null)
        {
            return null;
        }

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        user.PrimaryPhotoId = photo.Id;
        await db.SaveChangesAsync(cancellationToken);

        return photo.ToDto(storage, isPrimary: true);
    }
}
