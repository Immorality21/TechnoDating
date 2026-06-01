namespace TechnoDating.Contracts;

/// <summary>Match row for the admin tool, with both users' names and how it was created.</summary>
public record AdminMatchDto(
    Guid MatchId,
    Guid UserAId,
    string? UserAName,
    Guid UserBId,
    string? UserBName,
    MatchOrigin Origin,
    MatchStatus Status,
    DateTimeOffset CreatedAt);
