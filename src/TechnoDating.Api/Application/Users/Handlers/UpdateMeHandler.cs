using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using TechnoDating.Api.Application.Users.Requests;
using TechnoDating.Api.Infrastructure;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Users.Handlers;

public class UpdateMeHandler(TechnoDatingDbContext db) : IRequestHandler<UpdateMeRequest, UserProfileDto?>
{
    public async Task<UserProfileDto?> Handle(UpdateMeRequest request, CancellationToken cancellationToken)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var p = request.Profile;
        user.DisplayName = p.DisplayName;
        user.DateOfBirth = p.DateOfBirth;
        user.Gender = p.Gender;
        user.Bio = p.Bio;
        user.City = p.City;
        user.TopArtists = p.TopArtists.ToList();
        if (p.Longitude.HasValue && p.Latitude.HasValue)
        {
            user.Location = new Point(p.Longitude.Value, p.Latitude.Value) { SRID = 4326 };
        }
        user.LastActiveAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return user.ToProfileDto();
    }
}
