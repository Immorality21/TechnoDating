using MediatR;
using Microsoft.EntityFrameworkCore;
using TechnoDating.Api.Application.Discovery.Requests;
using TechnoDating.Api.Application.Photos;
using TechnoDating.Api.Application.Storage;
using TechnoDating.Api.Infrastructure;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Discovery.Handlers;

/// <summary>
/// The candidate feed. Ranked by shared festivals → music-taste overlap → everyone else.
/// Physical/home distance is deliberately NOT a signal — the geography that matters is which
/// shows you both attend, not where you each live. See docs/MATCHING.md.
/// </summary>
public class GetDiscoveryHandler(TechnoDatingDbContext db, IBlobStorage storage) : IRequestHandler<GetDiscoveryRequest, IReadOnlyList<MatchProfileDto>>
{
    public async Task<IReadOnlyList<MatchProfileDto>> Handle(GetDiscoveryRequest request, CancellationToken cancellationToken)
    {
        var meId = request.CurrentUserId;
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        // My committed festivals (Going/Ticketed) — the primary ranking signal.
        var myFestivalIds = await db.Attendances
            .AsNoTracking()
            .Where(a => a.UserId == meId
                && (a.Status == AttendanceStatus.Going || a.Status == AttendanceStatus.Ticketed))
            .Select(a => a.FestivalId)
            .ToListAsync(cancellationToken);
        var myFestivalSet = myFestivalIds.ToHashSet();

        var festivalNames = myFestivalSet.Count == 0
            ? new Dictionary<Guid, string>()
            : await db.Festivals
                .AsNoTracking()
                .Where(f => myFestivalSet.Contains(f.Id))
                .ToDictionaryAsync(f => f.Id, f => f.Name, cancellationToken);

        // My top artists — the secondary ranking signal (music-taste overlap).
        var myArtistSet = (await db.UserTopArtists
            .AsNoTracking()
            .Where(x => x.UserId == meId)
            .Select(x => x.ArtistId)
            .ToListAsync(cancellationToken)).ToHashSet();

        // Anyone I've already acted on (liked or passed) or already matched with drops out.
        var alreadyActedOn = await db.Likes
            .AsNoTracking()
            .Where(l => l.LikerId == meId)
            .Select(l => l.LikedId)
            .ToListAsync(cancellationToken);
        var alreadyMatched = await db.Matches
            .AsNoTracking()
            .Where(m => m.UserAId == meId || m.UserBId == meId)
            .Select(m => m.UserAId == meId ? m.UserBId : m.UserAId)
            .ToListAsync(cancellationToken);
        var excluded = alreadyActedOn.Concat(alreadyMatched).Append(meId).ToHashSet();

        var users = await db.Users
            .AsNoTracking()
            .Where(u => u.DisplayName != null && u.DateOfBirth != null && u.City != null)
            .Select(u => new { u.Id, u.DisplayName, u.DateOfBirth, u.City, u.LastActiveAt })
            .ToListAsync(cancellationToken);
        users = users.Where(u => !excluded.Contains(u.Id)).ToList();

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
                var sharedArtistCount = artists.Count(ar => myArtistSet.Contains(ar.Id));
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
                        PrimaryPhotoUrl: photoUrl),
                    SharedFestivalCount = sharedNames.Count,
                    SharedArtistCount = sharedArtistCount,
                    LastActive = u.LastActiveAt,
                };
            })
            // Festivals first, then taste, then most-recently-active so the feed is never empty.
            .OrderByDescending(x => x.SharedFestivalCount)
            .ThenByDescending(x => x.SharedArtistCount)
            .ThenByDescending(x => x.LastActive)
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
