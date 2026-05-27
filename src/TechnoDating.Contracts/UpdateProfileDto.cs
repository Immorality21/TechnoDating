namespace TechnoDating.Contracts;

public record UpdateProfileDto(
    string DisplayName,
    DateOnly DateOfBirth,
    string Gender,
    string? Bio,
    string City,
    IReadOnlyList<string> TopArtists,
    double? Longitude,
    double? Latitude);
