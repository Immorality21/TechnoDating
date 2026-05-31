namespace TechnoDating.Api.Infrastructure.Entities;

public class FestivalHeadlineArtist
{
    public Guid Id { get; set; }
    public Guid FestivalId { get; set; }
    public Guid ArtistId { get; set; }
    public int BillingOrder { get; set; }

    public Festival? Festival { get; set; }
    public Artist? Artist { get; set; }
}
