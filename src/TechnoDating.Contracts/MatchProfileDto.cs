namespace TechnoDating.Contracts;

/// <summary>
/// A candidate in the discovery feed (and festival "who's going" lists). Ranked by shared
/// festivals then music-taste overlap — physical/home distance is deliberately not a signal;
/// the geography that matters is which shows you both go to. See docs/MATCHING.md.
/// </summary>
public record MatchProfileDto(
    Guid Id,
    string DisplayName,
    int Age,
    string City,
    IReadOnlyList<ArtistRefDto> TopArtists,
    IReadOnlyList<string> CommonFestivals,
    string? PrimaryPhotoUrl);
