namespace TechnoDating.Contracts;

public record MatchProfileDto(
    Guid Id,
    string DisplayName,
    int Age,
    string City,
    IReadOnlyList<ArtistRefDto> TopArtists,
    IReadOnlyList<string> CommonFestivals,
    double DistanceKm,
    string? PrimaryPhotoUrl);
