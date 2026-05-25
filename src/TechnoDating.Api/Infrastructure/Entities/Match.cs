namespace TechnoDating.Api.Infrastructure.Entities;

public class Match
{
    public Guid Id { get; set; }
    public Guid UserAId { get; set; }
    public Guid UserBId { get; set; }
    public DateTimeOffset MatchedAt { get; set; }

    public User? UserA { get; set; }
    public User? UserB { get; set; }
}
