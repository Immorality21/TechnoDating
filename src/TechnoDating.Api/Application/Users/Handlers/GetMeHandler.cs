using MediatR;
using Microsoft.EntityFrameworkCore;
using TechnoDating.Api.Application.Users.Requests;
using TechnoDating.Api.Infrastructure;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Users.Handlers;

public class GetMeHandler(TechnoDatingDbContext db) : IRequestHandler<GetMeRequest, UserProfileDto?>
{
    public async Task<UserProfileDto?> Handle(GetMeRequest request, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var topArtists = await db.LoadTopArtistsAsync(user.Id, cancellationToken);
        return user.ToProfileDto(topArtists);
    }
}
