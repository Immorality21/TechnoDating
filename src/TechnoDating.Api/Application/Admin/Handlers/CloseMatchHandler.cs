using MediatR;
using Microsoft.EntityFrameworkCore;
using TechnoDating.Api.Application.Admin.Requests;
using TechnoDating.Api.Infrastructure;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Admin.Handlers;

public class CloseMatchHandler(TechnoDatingDbContext db) : IRequestHandler<CloseMatchRequest, bool>
{
    public async Task<bool> Handle(CloseMatchRequest request, CancellationToken cancellationToken)
    {
        var match = await db.Matches.FirstOrDefaultAsync(m => m.Id == request.MatchId, cancellationToken);
        if (match is null)
        {
            return false;
        }

        match.Status = MatchStatus.Closed;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
