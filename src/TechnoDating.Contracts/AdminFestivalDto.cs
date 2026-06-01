namespace TechnoDating.Contracts;

/// <summary>Festival row for the admin tool, with its headliner lineup.</summary>
public record AdminFestivalDto(
    Guid Id,
    string Name,
    DateOnly Date,
    string City,
    string Venue,
    IReadOnlyList<ArtistRefDto> Headliners);
