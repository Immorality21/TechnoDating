namespace TechnoDating.Contracts;

public record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessExpiresAt,
    UserProfileDto User);
