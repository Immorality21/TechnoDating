using MediatR;
using Microsoft.EntityFrameworkCore;
using TechnoDating.Api.Application.Admin.Requests;
using TechnoDating.Api.Application.Matches;
using TechnoDating.Api.Infrastructure;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Admin.Handlers;

/// <summary>
/// Admin force-match. Goes through the same <see cref="IMatchmaker"/> chokepoint as every other
/// producer — canonical pair, idempotent — tagged <see cref="MatchOrigin.Admin"/>. Never raw SQL.
/// </summary>
public class ForceMatchHandler(TechnoDatingDbContext db, IMatchmaker matchmaker) : IRequestHandler<ForceMatchRequest, AdminMatchDto?>
{
    public async Task<AdminMatchDto?> Handle(ForceMatchRequest request, CancellationToken cancellationToken)
    {
        var users = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == request.UserAId || u.Id == request.UserBId)
            .Select(u => new { u.Id, u.DisplayName })
            .ToListAsync(cancellationToken);

        // Both must exist (and be distinct, so two rows come back).
        if (users.Count < 2)
        {
            return null;
        }

        var match = await matchmaker.TryCreateMatchAsync(request.UserAId, request.UserBId, MatchOrigin.Admin, cancellationToken);

        var nameById = users.ToDictionary(u => u.Id, u => u.DisplayName);
        return new AdminMatchDto(
            match.Id,
            match.UserAId,
            nameById.GetValueOrDefault(match.UserAId),
            match.UserBId,
            nameById.GetValueOrDefault(match.UserBId),
            match.Origin,
            match.Status,
            match.CreatedAt);
    }
}
