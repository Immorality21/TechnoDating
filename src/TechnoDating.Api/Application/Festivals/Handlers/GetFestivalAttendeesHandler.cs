using MediatR;
using Microsoft.EntityFrameworkCore;
using TechnoDating.Api.Application.Festivals.Requests;
using TechnoDating.Api.Infrastructure;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Festivals.Handlers;

public class GetFestivalAttendeesHandler(TechnoDatingDbContext db) : IRequestHandler<GetFestivalAttendeesRequest, IReadOnlyList<MatchProfileDto>>
{
    public async Task<IReadOnlyList<MatchProfileDto>> Handle(GetFestivalAttendeesRequest request, CancellationToken cancellationToken)
    {
        var me = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == request.CurrentUserId)
            .Select(u => new { u.Location })
            .FirstOrDefaultAsync(cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var query =
            from a in db.Attendances.AsNoTracking()
            join u in db.Users.AsNoTracking() on a.UserId equals u.Id
            where a.FestivalId == request.FestivalId
                && a.UserId != request.CurrentUserId
                && u.DisplayName != null
                && u.DateOfBirth != null
                && u.City != null
            select new
            {
                u.Id,
                u.DisplayName,
                u.DateOfBirth,
                u.City,
                u.TopArtists,
                DistanceMeters = me!.Location != null && u.Location != null
                    ? u.Location.Distance(me.Location)
                    : (double?)null,
            };

        var rows = me?.Location is null
            ? await query.ToListAsync(cancellationToken)
            : await query.OrderBy(x => x.DistanceMeters ?? double.MaxValue).ToListAsync(cancellationToken);

        return rows.Select(u => new MatchProfileDto(
            u.Id,
            u.DisplayName!,
            Age: CalculateAge(u.DateOfBirth!.Value, today),
            u.City!,
            u.TopArtists,
            CommonFestivals: [],
            DistanceKm: u.DistanceMeters.HasValue ? Math.Round(u.DistanceMeters.Value / 1000.0, 1) : 0))
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
