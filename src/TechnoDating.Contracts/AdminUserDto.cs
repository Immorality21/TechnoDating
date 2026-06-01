namespace TechnoDating.Contracts;

/// <summary>User row for the admin tool's user list / troubleshooting view.</summary>
public record AdminUserDto(
    Guid Id,
    string? PhoneNumber,
    string? DisplayName,
    string? City,
    bool IsVerified,
    bool IsProfileComplete,
    DateTimeOffset CreatedAt);
