namespace TechnoDating.Contracts;

public record UpdateProfileDto(
    string DisplayName,
    DateOnly DateOfBirth,
    string Gender,
    string? Bio,
    string City,
    IReadOnlyList<Guid> TopArtistIds,
    double? Longitude,
    double? Latitude);
