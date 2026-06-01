namespace TechnoDating.Contracts;

/// <summary>
/// Result of submitting a like. <c>Matched</c> is true when this like reciprocated an
/// existing one and a match was formed — drives the "It's a match!" UX.
/// </summary>
public record LikeResultDto(bool Matched, Guid? MatchId);
