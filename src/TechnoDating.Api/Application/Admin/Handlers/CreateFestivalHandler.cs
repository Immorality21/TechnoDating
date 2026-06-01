using MediatR;
using TechnoDating.Api.Application.Admin.Requests;
using TechnoDating.Api.Infrastructure;
using TechnoDating.Api.Infrastructure.Entities;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Admin.Handlers;

public class CreateFestivalHandler(TechnoDatingDbContext db) : IRequestHandler<CreateFestivalRequest, AdminFestivalDto>
{
    public async Task<AdminFestivalDto> Handle(CreateFestivalRequest request, CancellationToken cancellationToken)
    {
        var p = request.Festival;
        var festival = new Festival
        {
            Id = Guid.NewGuid(),
            Name = p.Name,
            Date = p.Date,
            City = p.City,
            Venue = p.Venue,
        };
        db.Festivals.Add(festival);

        var headliners = await AdminFestivalMapping.ReplaceHeadlinersAsync(db, festival.Id, p.HeadlinerArtistIds, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return new AdminFestivalDto(festival.Id, festival.Name, festival.Date, festival.City, festival.Venue, headliners);
    }
}
