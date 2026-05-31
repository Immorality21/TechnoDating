namespace TechnoDating.Api.Infrastructure.Entities;

public class Artist
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Genre { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
