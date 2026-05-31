namespace TechnoDating.Contracts;

public record FestivalDto(
    Guid Id,
    string Name,
    DateOnly Date,
    string City,
    string Venue,
    IReadOnlyList<ArtistRefDto> HeadlineArtists,
    int AttendingCount,
    int MatchingArtistsCount,
    AttendanceStatus? MyStatus);
