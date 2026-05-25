using NetTopologySuite.Geometries;

namespace TechnoDating.Api.Infrastructure.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string City { get; set; } = string.Empty;
    public Point? Location { get; set; }
    public List<string> TopArtists { get; set; } = [];
    public bool IsVerified { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastActiveAt { get; set; }
}
