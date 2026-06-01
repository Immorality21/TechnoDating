using MediatR;
using Microsoft.EntityFrameworkCore;
using TechnoDating.Api.Application.Admin.Requests;
using TechnoDating.Api.Infrastructure;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Admin.Handlers;

public class UpdateFestivalHandler(TechnoDatingDbContext db) : IRequestHandler<UpdateFestivalRequest, AdminFestivalDto?>
{
    public async Task<AdminFestivalDto?> Handle(UpdateFestivalRequest request, CancellationToken cancellationToken)
    {
        var festival = await db.Festivals.FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);
        if (festival is null)
        {
            return null;
        }

        var p = request.Festival;
        festival.Name = p.Name;
        festival.Date = p.Date;
        festival.City = p.City;
        festival.Venue = p.Venue;

        var headliners = await AdminFestivalMapping.ReplaceHeadlinersAsync(db, festival.Id, p.HeadlinerArtistIds, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return new AdminFestivalDto(festival.Id, festival.Name, festival.Date, festival.City, festival.Venue, headliners);
    }
}
