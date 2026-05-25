namespace TechnoDating.Contracts;

public record MatchProfileDto(
    Guid Id,
    string DisplayName,
    int Age,
    string City,
    IReadOnlyList<string> TopArtists,
    IReadOnlyList<string> CommonFestivals,
    double DistanceKm);
