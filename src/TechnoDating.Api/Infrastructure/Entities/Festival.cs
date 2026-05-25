using NetTopologySuite.Geometries;

namespace TechnoDating.Api.Infrastructure.Entities;

public class Festival
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public string City { get; set; } = string.Empty;
    public string Venue { get; set; } = string.Empty;
    public List<string> HeadlineArtists { get; set; } = [];
    public Point? Location { get; set; }
}
