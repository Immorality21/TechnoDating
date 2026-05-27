namespace TechnoDating.Contracts;

public record UserProfileDto(
    Guid Id,
    string PhoneNumber,
    string? DisplayName,
    DateOnly? DateOfBirth,
    string? Gender,
    string? Bio,
    string? City,
    IReadOnlyList<string> TopArtists,
    bool IsVerified,
    bool IsProfileComplete);
