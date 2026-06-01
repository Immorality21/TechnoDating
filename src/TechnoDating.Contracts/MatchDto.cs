namespace TechnoDating.Contracts;

/// <summary>A confirmed mutual connection — returned by <c>GET /api/matches</c>.</summary>
public record MatchDto(
    Guid MatchId,
    Guid UserId,
    string DisplayName,
    int Age,
    string City,
    string? PrimaryPhotoUrl,
    DateTimeOffset MatchedAt);
