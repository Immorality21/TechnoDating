using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using TechnoDating.Api.Application.Matches.Requests;
using TechnoDating.Api.Infrastructure;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Matches.Handlers;

public class GetMatchesHandler(TechnoDatingDbContext db) : IRequestHandler<GetMatchesRequest, IReadOnlyList<MatchProfileDto>>
{
    public async Task<IReadOnlyList<MatchProfileDto>> Handle(GetMatchesRequest request, CancellationToken cancellationToken)
    {
        // Placeholder centre point until we have an authenticated "current user".
        var center = new Point(4.90, 52.37) { SRID = 4326 };
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var users = await db.Users
            .AsNoTracking()
            .Where(u => u.Location != null)
            .OrderBy(u => u.Location!.Distance(center))
            .Select(u => new
            {
                u.Id,
                u.DisplayName,
                u.DateOfBirth,
                u.City,
                u.TopArtists,
                DistanceMeters = u.Location!.Distance(center),
            })
            .ToListAsync(cancellationToken);

        var result = users.Select(u => new MatchProfileDto(
            u.Id,
            u.DisplayName,
            Age: CalculateAge(u.DateOfBirth, today),
            u.City,
            u.TopArtists,
            CommonFestivals: [],
            DistanceKm: Math.Round(u.DistanceMeters / 1000.0, 1)))
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
