using TechnoDating.Contracts;

namespace TechnoDating.Api.Infrastructure.Entities;

/// <summary>
/// A confirmed connection between two users — the stable contract the matching loop
/// revolves around. Created only via <c>IMatchmaker</c>; how a match is born (mutual like,
/// curation, algorithm) is recorded in <see cref="Origin"/> and never leaks downstream.
/// Pair is stored canonically (UserAId &lt; UserBId). See docs/MATCHING.md.
/// </summary>
public class Match
{
    public Guid Id { get; set; }
    public Guid UserAId { get; set; }
    public Guid UserBId { get; set; }
    public MatchOrigin Origin { get; set; }
    public MatchStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Nullable, unused for now. Reserved for Bumble/Hinge-style match expiry. See docs/MATCHING.md.</summary>
    public DateTimeOffset? ExpiresAt { get; set; }

    public User? UserA { get; set; }
    public User? UserB { get; set; }
}
