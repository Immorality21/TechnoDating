using MediatR;
using Microsoft.EntityFrameworkCore;
using TechnoDating.Api.Application.Artists.Requests;
using TechnoDating.Api.Infrastructure;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Artists.Handlers;

public class GetArtistsHandler(TechnoDatingDbContext db) : IRequestHandler<GetArtistsRequest, IReadOnlyList<ArtistDto>>
{
    public async Task<IReadOnlyList<ArtistDto>> Handle(GetArtistsRequest request, CancellationToken cancellationToken)
    {
        return await db.Artists
            .AsNoTracking()
            .OrderBy(a => a.Name)
            .Select(a => new ArtistDto(a.Id, a.Name, a.Genre))
            .ToListAsync(cancellationToken);
    }
}
