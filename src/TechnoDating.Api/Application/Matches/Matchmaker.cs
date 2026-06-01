using Microsoft.EntityFrameworkCore;
using TechnoDating.Api.Infrastructure;
using TechnoDating.Api.Infrastructure.Entities;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Matches;

public class Matchmaker(TechnoDatingDbContext db) : IMatchmaker
{
    public async Task<Match> TryCreateMatchAsync(Guid userA, Guid userB, MatchOrigin origin, CancellationToken cancellationToken)
    {
        if (userA == userB)
        {
            throw new ArgumentException("Cannot create a match between a user and themselves.", nameof(userB));
        }

        // Canonical ordering so (A,B) and (B,A) can never produce two rows.
        var (low, high) = userA.CompareTo(userB) < 0 ? (userA, userB) : (userB, userA);

        var existing = await db.Matches
            .FirstOrDefaultAsync(m => m.UserAId == low && m.UserBId == high, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var match = new Match
        {
            Id = Guid.NewGuid(),
            UserAId = low,
            UserBId = high,
            Origin = origin,
            Status = MatchStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        db.Matches.Add(match);
        await db.SaveChangesAsync(cancellationToken);
        return match;
    }
}
