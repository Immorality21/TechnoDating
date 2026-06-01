using TechnoDating.Api.Application.Discovery.Handlers;
using TechnoDating.Api.Application.Discovery.Requests;
using TechnoDating.Api.Infrastructure;
using TechnoDating.Api.Infrastructure.Entities;
using TechnoDating.Contracts;
using Xunit;

namespace TechnoDating.Api.Tests;

public class GetDiscoveryHandlerTests
{
    [Fact]
    public async Task Ranks_FestivalOverlap_ThenTaste_ThenRest_AndExcludesActedOn()
    {
        using var db = TestDb.NewContext();

        var me = Guid.NewGuid();
        var festivalSharer = Guid.NewGuid(); // shares a festival with me
        var tasteSharer = Guid.NewGuid();    // shares a top artist with me
        var stranger = Guid.NewGuid();       // no overlap
        var liked = Guid.NewGuid();           // already liked → excluded

        var artist = new Artist { Id = Guid.NewGuid(), Name = "Artist One", Slug = "artist-one" };
        var festival = new Festival { Id = Guid.NewGuid(), Name = "Fest One", Date = new DateOnly(2026, 7, 4), City = "Amsterdam", Venue = "Venue" };
        db.Artists.Add(artist);
        db.Festivals.Add(festival);

        foreach (var (id, name) in new[] { (me, "Me"), (festivalSharer, "Fest"), (tasteSharer, "Taste"), (stranger, "Stranger"), (liked, "Liked") })
        {
            db.Users.Add(new User
            {
                Id = id,
                UserName = name,
                DisplayName = name,
                DateOfBirth = new DateOnly(1995, 1, 1),
                Gender = "female",
                City = "Amsterdam",
            });
        }

        var now = DateTimeOffset.UtcNow;
        db.Attendances.Add(new UserFestivalAttendance { Id = Guid.NewGuid(), UserId = me, FestivalId = festival.Id, Status = AttendanceStatus.Going, CreatedAt = now, UpdatedAt = now });
        db.UserTopArtists.Add(new UserTopArtist { Id = Guid.NewGuid(), UserId = me, ArtistId = artist.Id, Rank = 1 });

        db.Attendances.Add(new UserFestivalAttendance { Id = Guid.NewGuid(), UserId = festivalSharer, FestivalId = festival.Id, Status = AttendanceStatus.Going, CreatedAt = now, UpdatedAt = now });
        db.UserTopArtists.Add(new UserTopArtist { Id = Guid.NewGuid(), UserId = tasteSharer, ArtistId = artist.Id, Rank = 1 });
        db.Likes.Add(new Like { Id = Guid.NewGuid(), LikerId = me, LikedId = liked, Kind = LikeKind.Like, CreatedAt = now });

        await db.SaveChangesAsync();

        var handler = new GetDiscoveryHandler(db, new FakeBlobStorage());
        var result = await handler.Handle(new GetDiscoveryRequest(me), CancellationToken.None);

        Assert.Equal(new[] { festivalSharer, tasteSharer, stranger }, result.Select(r => r.Id).ToArray());
        Assert.DoesNotContain(result, r => r.Id == liked);
        Assert.Contains("Fest One", result[0].CommonFestivals);
    }
}
