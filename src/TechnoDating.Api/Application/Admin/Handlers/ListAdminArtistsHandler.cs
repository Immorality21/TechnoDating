using MediatR;
using Microsoft.EntityFrameworkCore;
using TechnoDating.Api.Application.Admin.Requests;
using TechnoDating.Api.Infrastructure;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Admin.Handlers;

public class ListAdminArtistsHandler(TechnoDatingDbContext db) : IRequestHandler<ListAdminArtistsRequest, IReadOnlyList<ArtistDto>>
{
    public async Task<IReadOnlyList<ArtistDto>> Handle(ListAdminArtistsRequest request, CancellationToken cancellationToken)
    {
        return await db.Artists
            .AsNoTracking()
            .OrderBy(a => a.Genre)
            .ThenBy(a => a.Name)
            .Select(a => new ArtistDto(a.Id, a.Name, a.Genre))
            .ToListAsync(cancellationToken);
    }
}
