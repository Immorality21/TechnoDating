using MediatR;
using Microsoft.EntityFrameworkCore;
using TechnoDating.Api.Application.Matches.Requests;
using TechnoDating.Api.Application.Photos;
using TechnoDating.Api.Application.Storage;
using TechnoDating.Api.Infrastructure;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Matches.Handlers;

public class GetMatchesHandler(TechnoDatingDbContext db, IBlobStorage storage) : IRequestHandler<GetMatchesRequest, IReadOnlyList<MatchProfileDto>>
{
    public async Task<IReadOnlyList<MatchProfileDto>> Handle(GetMatchesRequest request, CancellationToken cancellationToken)
    {
        var me = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == request.CurrentUserId)
            .Select(u => new { u.Location })
            .FirstOrDefaultAsync(cancellationToken);

        if (me?.Location is null)
        {
            return [];
        }

        var center = me.Location;
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var myCommittedFestivalIds = await db.Attendances
            .AsNoTracking()
            .Where(a => a.UserId == request.CurrentUserId
                && (a.Status == AttendanceStatus.Going || a.Status == AttendanceStatus.Ticketed))
            .Select(a => a.FestivalId)
            .ToListAsync(cancellationToken);
        var myFestivalSet = myCommittedFestivalIds.ToHashSet();

        var festivalNames = myFestivalSet.Count == 0
            ? new Dictionary<Guid, string>()
            : await db.Festivals
                .AsNoTracking()
                .Where(f => myFestivalSet.Contains(f.Id))
                .ToDictionaryAsync(f => f.Id, f => f.Name, cancellationToken);

        var users = await db.Users
            .AsNoTracking()
            .Where(u => u.Id != request.CurrentUserId
                && u.Location != null
                && u.DisplayName != null
                && u.DateOfBirth != null
                && u.City != null)
            .Select(u => new
            {
                u.Id,
                u.DisplayName,
                u.DateOfBirth,
                u.City,
                DistanceMeters = u.Location!.Distance(center),
            })
            .ToListAsync(cancellationToken);

        if (users.Count == 0)
        {
            return [];
        }

        var userIds = users.Select(u => u.Id).ToList();

        var topArtistRows = await db.UserTopArtists
            .AsNoTracking()
            .Where(x => userIds.Contains(x.UserId))
            .OrderBy(x => x.Rank)
            .Join(db.Artists, x => x.ArtistId, a => a.Id, (x, a) => new { x.UserId, Artist = new ArtistRefDto(a.Id, a.Name) })
            .ToListAsync(cancellationToken);
        var topArtistsByUser = topArtistRows
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<ArtistRefDto>)g.Select(x => x.Artist).ToList());

        var theirOverlappingFestivals = new Dictionary<Guid, List<Guid>>();
        if (myFestivalSet.Count > 0)
        {
            theirOverlappingFestivals = await db.Attendances
                .AsNoTracking()
                .Where(a => userIds.Contains(a.UserId)
                    && myFestivalSet.Contains(a.FestivalId)
                    && (a.Status == AttendanceStatus.Going || a.Status == AttendanceStatus.Ticketed))
                .GroupBy(a => a.UserId)
                .Select(g => new { UserId = g.Key, FestivalIds = g.Select(a => a.FestivalId).ToList() })
                .ToDictionaryAsync(x => x.UserId, x => x.FestivalIds, cancellationToken);
        }

        var primaryPhotoUrls = await db.LoadPrimaryPhotoCardUrlsAsync(storage, userIds, cancellationToken);

        var empty = (IReadOnlyList<ArtistRefDto>)Array.Empty<ArtistRefDto>();

        var result = users
            .Select(u =>
            {
                var sharedIds = theirOverlappingFestivals.TryGetValue(u.Id, out var ids) ? ids : new List<Guid>();
                var sharedNames = sharedIds
                    .Select(id => festivalNames.TryGetValue(id, out var name) ? name : null)
                    .Where(n => n is not null)
                    .Cast<string>()
                    .ToList();
                var artists = topArtistsByUser.TryGetValue(u.Id, out var a) ? a : empty;
                var photoUrl = primaryPhotoUrls.TryGetValue(u.Id, out var url) ? url : null;

                return new
                {
                    Profile = new MatchProfileDto(
                        u.Id,
                        u.DisplayName!,
                        Age: CalculateAge(u.DateOfBirth!.Value, today),
                        u.City!,
                        artists,
                        CommonFestivals: sharedNames,
                        DistanceKm: Math.Round(u.DistanceMeters / 1000.0, 1),
                        PrimaryPhotoUrl: photoUrl),
                    SharedCount = sharedNames.Count,
                    Distance = u.DistanceMeters,
                };
            })
            .OrderByDescending(x => x.SharedCount)
            .ThenBy(x => x.Distance)
            .Select(x => x.Profile)
            .ToList();

        return result;
    }

    private static int CalculateAge(DateOnly dob, DateOnly today)
    {
        var age = today.Year - dob.Year;
        if (today < dob.AddYears(age))
        {
            age--;
        }
        return age;
    }
}
