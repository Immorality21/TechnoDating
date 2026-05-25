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

        var result = festivals.Select(f => new FestivalDto(
            f.Id,
            f.Name,
            f.Date,
            f.City,
            f.Venue,
            f.HeadlineArtists,
            MatchingPeopleCount: 0))
            .ToList();

        return result;
    }
}
