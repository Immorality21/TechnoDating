using Microsoft.EntityFrameworkCore;
using TechnoDating.Api.Infrastructure;
using TechnoDating.Api.Infrastructure.Entities;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Users;

public static class UserMappingExtensions
{
    public static UserProfileDto ToProfileDto(
        this User user,
        IReadOnlyList<ArtistRefDto> topArtists,
        IReadOnlyList<PhotoDto> photos)
    {
        var primary = photos.FirstOrDefault(p => p.IsPrimary);
        return new UserProfileDto(
            user.Id,
            user.PhoneNumber ?? string.Empty,
            user.DisplayName,
            user.DateOfBirth,
            user.Gender,
            user.Bio,
            user.City,
            topArtists,
            photos,
            primary?.CardUrl,
            user.IsVerified,
            user.IsProfileComplete,
            user.Goal);
    }

    public static async Task<IReadOnlyList<ArtistRefDto>> LoadTopArtistsAsync(
        this TechnoDatingDbContext db,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await db.UserTopArtists
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Rank)
            .Join(
                db.Artists,
                x => x.ArtistId,
                a => a.Id,
                (x, a) => new ArtistRefDto(a.Id, a.Name))
            .ToListAsync(cancellationToken);
    }
}
