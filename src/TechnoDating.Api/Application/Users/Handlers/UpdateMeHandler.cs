using MediatR;
using Microsoft.EntityFrameworkCore;
using TechnoDating.Api.Application.Photos;
using TechnoDating.Api.Application.Storage;
using TechnoDating.Api.Application.Users.Requests;
using TechnoDating.Api.Infrastructure;
using TechnoDating.Api.Infrastructure.Entities;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Users.Handlers;

public class UpdateMeHandler(TechnoDatingDbContext db, IBlobStorage storage) : IRequestHandler<UpdateMeRequest, UserProfileDto?>
{
    public async Task<UserProfileDto?> Handle(UpdateMeRequest request, CancellationToken cancellationToken)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var p = request.Profile;
        user.DisplayName = p.DisplayName;
        user.DateOfBirth = p.DateOfBirth;
        user.Gender = p.Gender;
        user.Bio = p.Bio;
        user.City = p.City;
        user.LastActiveAt = DateTimeOffset.UtcNow;

        await db.UserTopArtists.Where(x => x.UserId == user.Id).ExecuteDeleteAsync(cancellationToken);

        if (p.TopArtistIds.Count > 0)
        {
            var requestedIds = p.TopArtistIds.Distinct().ToList();
            var validIds = await db.Artists
                .AsNoTracking()
                .Where(a => requestedIds.Contains(a.Id))
                .Select(a => a.Id)
                .ToListAsync(cancellationToken);
            var validSet = validIds.ToHashSet();

            var rank = 1;
            foreach (var artistId in requestedIds.Where(id => validSet.Contains(id)))
            {
                db.UserTopArtists.Add(new UserTopArtist
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    ArtistId = artistId,
                    Rank = rank++,
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        var topArtists = await db.LoadTopArtistsAsync(user.Id, cancellationToken);
        var photos = await db.LoadPhotosAsync(storage, user.Id, cancellationToken);
        return user.ToProfileDto(topArtists, photos);
    }
}
