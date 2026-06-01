using MediatR;
using Microsoft.EntityFrameworkCore;
using TechnoDating.Api.Application.Festivals.Requests;
using TechnoDating.Api.Application.Photos;
using TechnoDating.Api.Application.Storage;
using TechnoDating.Api.Infrastructure;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Festivals.Handlers;

/// <summary>
/// The "who's going" list for a festival. Everyone here already shares this festival, so there
/// is no distance/proximity ranking — ordered by display name. See docs/MATCHING.md.
/// </summary>
public class GetFestivalAttendeesHandler(TechnoDatingDbContext db, IBlobStorage storage) : IRequestHandler<GetFestivalAttendeesRequest, IReadOnlyList<MatchProfileDto>>
{
    public async Task<IReadOnlyList<MatchProfileDto>> Handle(GetFestivalAttendeesRequest request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var rows = await (
            from a in db.Attendances.AsNoTracking()
            join u in db.Users.AsNoTracking() on a.UserId equals u.Id
            where a.FestivalId == request.FestivalId
                && a.UserId != request.CurrentUserId
                && u.DisplayName != null
                && u.DateOfBirth != null
                && u.City != null
            orderby u.DisplayName
            select new
            {
                u.Id,
                u.DisplayName,
                u.DateOfBirth,
                u.City,
            }).ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            return [];
        }

        var attendeeIds = rows.Select(r => r.Id).ToList();
        var topArtistRows = await db.UserTopArtists
            .AsNoTracking()
            .Where(x => attendeeIds.Contains(x.UserId))
            .OrderBy(x => x.Rank)
            .Join(db.Artists, x => x.ArtistId, a => a.Id, (x, a) => new { x.UserId, Artist = new ArtistRefDto(a.Id, a.Name) })
            .ToListAsync(cancellationToken);
        var topArtistsByUser = topArtistRows
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<ArtistRefDto>)g.Select(x => x.Artist).ToList());

        var primaryPhotoUrls = await db.LoadPrimaryPhotoCardUrlsAsync(storage, attendeeIds, cancellationToken);
        var empty = (IReadOnlyList<ArtistRefDto>)Array.Empty<ArtistRefDto>();

        return rows.Select(u => new MatchProfileDto(
            u.Id,
            u.DisplayName!,
            Age: CalculateAge(u.DateOfBirth!.Value, today),
            u.City!,
            topArtistsByUser.TryGetValue(u.Id, out var artists) ? artists : empty,
            CommonFestivals: [],
            PrimaryPhotoUrl: primaryPhotoUrls.TryGetValue(u.Id, out var url) ? url : null))
            .ToList();
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
