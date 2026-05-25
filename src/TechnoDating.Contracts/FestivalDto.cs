namespace TechnoDating.Contracts;

public record FestivalDto(
    Guid Id,
    string Name,
    DateOnly Date,
    string City,
    string Venue,
    IReadOnlyList<string> HeadlineArtists,
    int MatchingPeopleCount);
