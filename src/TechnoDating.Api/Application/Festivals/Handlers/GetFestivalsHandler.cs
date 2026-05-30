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

        var attendingCounts = await db.Attendances
            .AsNoTracking()
            .Where(a => festivalIds.Contains(a.FestivalId) && (a.Status == AttendanceStatus.Going || a.Status == AttendanceStatus.Ticketed))
            .GroupBy(a => a.FestivalId)
            .Select(g => new { FestivalId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.FestivalId, x => x.Count, cancellationToken);

        var myStatuses = await db.Attendances
            .AsNoTracking()
            .Where(a => a.UserId == request.CurrentUserId && festivalIds.Contains(a.FestivalId))
            .Select(a => new { a.FestivalId, a.Status })
            .ToDictionaryAsync(x => x.FestivalId, x => x.Status, cancellationToken);

        var myTopArtists = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == request.CurrentUserId)
            .Select(u => u.TopArtists)
            .FirstOrDefaultAsync(cancellationToken) ?? [];
        var myTopArtistsSet = new HashSet<string>(myTopArtists, StringComparer.OrdinalIgnoreCase);

        return festivals.Select(f => new FestivalDto(
            f.Id,
            f.Name,
            f.Date,
            f.City,
            f.Venue,
            f.HeadlineArtists,
            AttendingCount: attendingCounts.TryGetValue(f.Id, out var c) ? c : 0,
            MatchingArtistsCount: f.HeadlineArtists.Count(a => myTopArtistsSet.Contains(a)),
            MyStatus: myStatuses.TryGetValue(f.Id, out var s) ? s : null))
            .ToList();
    }
}
