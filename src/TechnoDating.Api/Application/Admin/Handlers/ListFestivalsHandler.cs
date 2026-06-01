using MediatR;
using Microsoft.EntityFrameworkCore;
using TechnoDating.Api.Application.Admin.Requests;
using TechnoDating.Api.Infrastructure;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Admin.Handlers;

public class ListFestivalsHandler(TechnoDatingDbContext db) : IRequestHandler<ListFestivalsRequest, IReadOnlyList<AdminFestivalDto>>
{
    public async Task<IReadOnlyList<AdminFestivalDto>> Handle(ListFestivalsRequest request, CancellationToken cancellationToken)
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
            .Where(x => festivalIds.Contains(x.FestivalId))
            .OrderBy(x => x.BillingOrder)
            .Join(db.Artists, x => x.ArtistId, a => a.Id, (x, a) => new { x.FestivalId, Artist = new ArtistRefDto(a.Id, a.Name) })
            .ToListAsync(cancellationToken);
        var headlinersByFestival = headlinerRows
            .GroupBy(x => x.FestivalId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<ArtistRefDto>)g.Select(x => x.Artist).ToList());

        var empty = (IReadOnlyList<ArtistRefDto>)Array.Empty<ArtistRefDto>();
        return festivals.Select(f => new AdminFestivalDto(
            f.Id,
            f.Name,
            f.Date,
            f.City,
            f.Venue,
            headlinersByFestival.TryGetValue(f.Id, out var h) ? h : empty)).ToList();
    }
}
