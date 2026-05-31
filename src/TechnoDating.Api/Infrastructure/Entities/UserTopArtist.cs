namespace TechnoDating.Api.Infrastructure.Entities;

public class UserTopArtist
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ArtistId { get; set; }
    public int Rank { get; set; }

    public User? User { get; set; }
    public Artist? Artist { get; set; }
}
