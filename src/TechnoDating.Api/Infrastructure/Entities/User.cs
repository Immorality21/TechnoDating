using Microsoft.AspNetCore.Identity;
using NetTopologySuite.Geometries;

namespace TechnoDating.Api.Infrastructure.Entities;

public class User : IdentityUser<Guid>
{
    public string? DisplayName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Bio { get; set; }
    public string? City { get; set; }
    public Point? Location { get; set; }
    public bool IsVerified { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastActiveAt { get; set; }

    public bool IsProfileComplete =>
        !string.IsNullOrWhiteSpace(DisplayName)
        && DateOfBirth.HasValue
        && !string.IsNullOrWhiteSpace(Gender)
        && !string.IsNullOrWhiteSpace(City);
}
