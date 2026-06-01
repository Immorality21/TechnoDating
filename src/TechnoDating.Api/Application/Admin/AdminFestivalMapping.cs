using Microsoft.EntityFrameworkCore;
using TechnoDating.Api.Infrastructure;
using TechnoDating.Api.Infrastructure.Entities;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Admin;

internal static class AdminFestivalMapping
{
    /// <summary>
    /// Atomically replaces a festival's headliners with the given (ordered, validated) artist ids
    /// and returns the resulting lineup. Uses tracked RemoveRange/Add (no ExecuteDelete) so it
    /// works for both new and existing festivals and stays unit-testable.
    /// </summary>
    public static async Task<IReadOnlyList<ArtistRefDto>> ReplaceHeadlinersAsync(
        TechnoDatingDbContext db,
        Guid festivalId,
        IReadOnlyList<Guid> artistIds,
        CancellationToken cancellationToken)
    {
        var existing = await db.FestivalHeadlineArtists
            .Where(x => x.FestivalId == festivalId)
            .ToListAsync(cancellationToken);
        if (existing.Count > 0)
        {
            db.FestivalHeadlineArtists.RemoveRange(existing);
        }

        if (artistIds.Count == 0)
        {
            return Array.Empty<ArtistRefDto>();
        }

        var requested = artistIds.Distinct().ToList();
        var validNames = await db.Artists
            .AsNoTracking()
            .Where(a => requested.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => a.Name, cancellationToken);

        var lineup = new List<ArtistRefDto>();
        var order = 0;
        foreach (var id in requested)
        {
            if (!validNames.TryGetValue(id, out var name))
            {
                continue;
            }
            db.FestivalHeadlineArtists.Add(new FestivalHeadlineArtist
            {
                Id = Guid.NewGuid(),
                FestivalId = festivalId,
                ArtistId = id,
                BillingOrder = order++,
            });
            lineup.Add(new ArtistRefDto(id, name));
        }
        return lineup;
    }
}
