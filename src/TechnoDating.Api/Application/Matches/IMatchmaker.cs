using TechnoDating.Api.Infrastructure.Entities;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Matches;

/// <summary>
/// The single chokepoint through which every match is created, regardless of policy
/// (mutual-like today; curated/algorithmic/admin later). Producers call this; consumers
/// (matches list, chat) read <see cref="Match"/>. See docs/MATCHING.md.
/// </summary>
public interface IMatchmaker
{
    /// <summary>
    /// Idempotently create (or return the existing) match for a user pair. Canonicalizes
    /// the pair so (A,B) and (B,A) collapse to one row.
    /// </summary>
    Task<Match> TryCreateMatchAsync(Guid userA, Guid userB, MatchOrigin origin, CancellationToken cancellationToken);
}
