using Microsoft.EntityFrameworkCore;
using TechnoDating.Api.Application.Likes.Handlers;
using TechnoDating.Api.Application.Likes.Requests;
using TechnoDating.Api.Application.Matches;
using TechnoDating.Api.Infrastructure;
using TechnoDating.Api.Infrastructure.Entities;
using TechnoDating.Contracts;
using Xunit;

namespace TechnoDating.Api.Tests;

public class SubmitLikeHandlerTests
{
    private static async Task<(Guid A, Guid B)> SeedTwoUsersAsync(TechnoDatingDbContext db)
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        db.Users.Add(new User { Id = a, UserName = "a", DisplayName = "A" });
        db.Users.Add(new User { Id = b, UserName = "b", DisplayName = "B" });
        await db.SaveChangesAsync();
        return (a, b);
    }

    private static SubmitLikeHandler NewHandler(TechnoDatingDbContext db) => new(db, new Matchmaker(db));

    [Fact]
    public async Task OneWayLike_DoesNotMatch()
    {
        using var db = TestDb.NewContext();
        var (a, b) = await SeedTwoUsersAsync(db);
        var handler = NewHandler(db);

        var result = await handler.Handle(new SubmitLikeRequest(a, b, LikeKind.Like), CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result!.Matched);
        Assert.Null(result.MatchId);
        Assert.Empty(db.Matches);
        Assert.Single(db.Likes);
    }

    [Fact]
    public async Task ReciprocalLike_CreatesMatch()
    {
        using var db = TestDb.NewContext();
        var (a, b) = await SeedTwoUsersAsync(db);
        var handler = NewHandler(db);

        await handler.Handle(new SubmitLikeRequest(a, b, LikeKind.Like), CancellationToken.None);
        var result = await handler.Handle(new SubmitLikeRequest(b, a, LikeKind.Like), CancellationToken.None);

        Assert.True(result!.Matched);
        Assert.NotNull(result.MatchId);
        Assert.Single(db.Matches);
        var match = await db.Matches.FirstAsync();
        Assert.Equal(MatchOrigin.MutualLike, match.Origin);
    }

    [Fact]
    public async Task Pass_DoesNotMatch_EvenIfOtherAlreadyLiked()
    {
        using var db = TestDb.NewContext();
        var (a, b) = await SeedTwoUsersAsync(db);
        var handler = NewHandler(db);

        await handler.Handle(new SubmitLikeRequest(a, b, LikeKind.Like), CancellationToken.None);
        var result = await handler.Handle(new SubmitLikeRequest(b, a, LikeKind.Pass), CancellationToken.None);

        Assert.False(result!.Matched);
        Assert.Empty(db.Matches);
    }

    [Fact]
    public async Task UnknownTarget_ReturnsNull()
    {
        using var db = TestDb.NewContext();
        var a = Guid.NewGuid();
        db.Users.Add(new User { Id = a, UserName = "a" });
        await db.SaveChangesAsync();
        var handler = NewHandler(db);

        var result = await handler.Handle(new SubmitLikeRequest(a, Guid.NewGuid(), LikeKind.Like), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Reliking_UpdatesKind_WithoutDuplicateRow()
    {
        using var db = TestDb.NewContext();
        var (a, b) = await SeedTwoUsersAsync(db);
        var handler = NewHandler(db);

        await handler.Handle(new SubmitLikeRequest(a, b, LikeKind.Pass), CancellationToken.None);
        await handler.Handle(new SubmitLikeRequest(a, b, LikeKind.Like), CancellationToken.None);

        Assert.Single(db.Likes);
        var like = await db.Likes.FirstAsync();
        Assert.Equal(LikeKind.Like, like.Kind);
    }
}
