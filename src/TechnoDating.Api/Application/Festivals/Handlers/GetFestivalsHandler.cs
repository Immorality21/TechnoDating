using MediatR;
using Microsoft.EntityFrameworkCore;
using TechnoDating.Api.Application.Festivals.Requests;
using TechnoDating.Api.Infrastructure;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Festivals.Handlers;

public class GetFestivalsHandler(TechnoDatingDbContext db) : IRequestHandler<GetFestivalsRequest, IReadOnlyList<FestivalDto>>
{
    public async Task<IReadOnlyList<FestivalDto>> Handle(GetFestivalsRequest request, CancellationToken cancellationToken)
    {
        var festivals = await db.Festivals
            .AsNoTracking()
            .OrderBy(f => f.Date)
            .ToListAsync(cancellationToken);

        if (festivals.Count == 0)
        {
            return [];
        }

        var festivalIds = festivals.Select(f => f.Id).ToList();

        var headlinerRows = await db.FestivalHeadlineArtists
            .AsNoTracking()
            .Where(fha => festivalIds.Contains(fha.FestivalId))
            .OrderBy(fha => fha.BillingOrder)
            .Join(
                db.Artists,
                fha => fha.ArtistId,
                a => a.Id,
                (fha, a) => new { fha.FestivalId, Artist = new ArtistRefDto(a.Id, a.Name) })
            .ToListAsync(cancellationToken);
        var headlinersByFestival = headlinerRows
            .GroupBy(x => x.FestivalId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<ArtistRefDto>)g.Select(x => x.Artist).ToList());

        var attendingCounts = await db.Attendances
            .AsNoTracking()
            .Where(a => festivalIds.Contains(a.FestivalId)
                && (a.Status == AttendanceStatus.Going || a.Status == AttendanceStatus.Ticketed))
            .GroupBy(a => a.FestivalId)
            .Select(g => new { FestivalId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.FestivalId, x => x.Count, cancellationToken);

        var myStatuses = await db.Attendances
            .AsNoTracking()
            .Where(a => a.UserId == request.CurrentUserId && festivalIds.Contains(a.FestivalId))
            .Select(a => new { a.FestivalId, a.Status })
            .ToDictionaryAsync(x => x.FestivalId, x => x.Status, cancellationToken);

        var myTopArtistIds = (await db.UserTopArtists
            .AsNoTracking()
            .Where(x => x.UserId == request.CurrentUserId)
            .Select(x => x.ArtistId)
            .ToListAsync(cancellationToken))
            .ToHashSet();

        var empty = (IReadOnlyList<ArtistRefDto>)Array.Empty<ArtistRefDto>();

        return festivals.Select(f =>
        {
            var headliners = headlinersByFestival.TryGetValue(f.Id, out var list) ? list : empty;
            return new FestivalDto(
                f.Id,
                f.Name,
                f.Date,
                f.City,
                f.Venue,
                headliners,
                AttendingCount: attendingCounts.TryGetValue(f.Id, out var c) ? c : 0,
                MatchingArtistsCount: headliners.Count(a => myTopArtistIds.Contains(a.Id)),
                MyStatus: myStatuses.TryGetValue(f.Id, out var s) ? s : null);
        }).ToList();
    }
}
