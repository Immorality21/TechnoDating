namespace TechnoDating.Contracts;

public record UserProfileDto(
    Guid Id,
    string PhoneNumber,
    string? DisplayName,
    DateOnly? DateOfBirth,
    string? Gender,
    string? Bio,
    string? City,
    IReadOnlyList<ArtistRefDto> TopArtists,
    bool IsVerified,
    bool IsProfileComplete);
