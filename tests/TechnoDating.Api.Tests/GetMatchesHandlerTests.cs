using TechnoDating.Api.Application.Matches.Handlers;
using TechnoDating.Api.Application.Matches.Requests;
using TechnoDating.Api.Infrastructure.Entities;
using TechnoDating.Contracts;
using Xunit;

namespace TechnoDating.Api.Tests;

public class GetMatchesHandlerTests
{
    [Fact]
    public async Task ReturnsTheOtherUser_RegardlessOfPairSlot()
    {
        using var db = TestDb.NewContext();
        var me = Guid.NewGuid();
        var other = Guid.NewGuid();
        db.Users.Add(new User { Id = me, UserName = "me", DisplayName = "Me", DateOfBirth = new DateOnly(1990, 1, 1), City = "Amsterdam" });
        db.Users.Add(new User { Id = other, UserName = "other", DisplayName = "Maud", DateOfBirth = new DateOnly(1996, 8, 21), City = "Amsterdam" });
        // 'me' deliberately in the UserBId slot to prove both pair columns are checked.
        db.Matches.Add(new Match { Id = Guid.NewGuid(), UserAId = other, UserBId = me, Origin = MatchOrigin.MutualLike, Status = MatchStatus.Active, CreatedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();

        var handler = new GetMatchesHandler(db, new FakeBlobStorage());
        var result = await handler.Handle(new GetMatchesRequest(me), CancellationToken.None);

        var match = Assert.Single(result);
        Assert.Equal(other, match.UserId);
        Assert.Equal("Maud", match.DisplayName);
    }

    [Fact]
    public async Task ExcludesClosedMatches()
    {
        using var db = TestDb.NewContext();
        var me = Guid.NewGuid();
        var other = Guid.NewGuid();
        db.Users.Add(new User { Id = me, UserName = "me", DisplayName = "Me", DateOfBirth = new DateOnly(1990, 1, 1), City = "Amsterdam" });
        db.Users.Add(new User { Id = other, UserName = "other", DisplayName = "Other", DateOfBirth = new DateOnly(1996, 1, 1), City = "Amsterdam" });
        db.Matches.Add(new Match { Id = Guid.NewGuid(), UserAId = me, UserBId = other, Origin = MatchOrigin.MutualLike, Status = MatchStatus.Closed, CreatedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();

        var handler = new GetMatchesHandler(db, new FakeBlobStorage());
        var result = await handler.Handle(new GetMatchesRequest(me), CancellationToken.None);

        Assert.Empty(result);
    }
}
