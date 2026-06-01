using TechnoDating.Contracts;

namespace TechnoDating.Api.Infrastructure.Entities;

/// <summary>
/// A directional interest signal (like or pass). Append-only signal store — deliberately
/// decoupled from <see cref="Match"/> so the matching policy can change without a migration.
/// See docs/MATCHING.md.
/// </summary>
public class Like
{
    public Guid Id { get; set; }
    public Guid LikerId { get; set; }
    public Guid LikedId { get; set; }
    public LikeKind Kind { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public User? Liker { get; set; }
    public User? Liked { get; set; }
}
