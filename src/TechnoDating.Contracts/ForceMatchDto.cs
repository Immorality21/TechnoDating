namespace TechnoDating.Contracts;

/// <summary>Body for the admin force-match endpoint — creates a match via the matchmaker (Admin origin).</summary>
public record ForceMatchDto(Guid UserAId, Guid UserBId);
