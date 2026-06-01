using Microsoft.EntityFrameworkCore;
using TechnoDating.Api.Application.Admin.Handlers;
using TechnoDating.Api.Application.Admin.Requests;
using TechnoDating.Api.Application.Matches;
using TechnoDating.Api.Infrastructure;
using TechnoDating.Api.Infrastructure.Entities;
using TechnoDating.Contracts;
using Xunit;

namespace TechnoDating.Api.Tests;

public class ForceMatchHandlerTests
{
    private static async Task<(Guid A, Guid B)> SeedTwoUsersAsync(TechnoDatingDbContext db)
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        db.Users.Add(new User { Id = a, UserName = "a", DisplayName = "Ada" });
        db.Users.Add(new User { Id = b, UserName = "b", DisplayName = "Ben" });
        await db.SaveChangesAsync();
        return (a, b);
    }

    [Fact]
    public async Task CreatesMatch_WithAdminOrigin()
    {
        using var db = TestDb.NewContext();
        var (a, b) = await SeedTwoUsersAsync(db);
        var handler = new ForceMatchHandler(db, new Matchmaker(db));

        var result = await handler.Handle(new ForceMatchRequest(a, b), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(MatchOrigin.Admin, result!.Origin);
        Assert.Single(db.Matches);
    }

    [Fact]
    public async Task IsIdempotent()
    {
        using var db = TestDb.NewContext();
        var (a, b) = await SeedTwoUsersAsync(db);
        var handler = new ForceMatchHandler(db, new Matchmaker(db));

        var first = await handler.Handle(new ForceMatchRequest(a, b), CancellationToken.None);
        var second = await handler.Handle(new ForceMatchRequest(b, a), CancellationToken.None);

        Assert.Equal(first!.MatchId, second!.MatchId);
        Assert.Single(db.Matches);
    }

    [Fact]
    public async Task ReturnsNull_WhenAUserDoesNotExist()
    {
        using var db = TestDb.NewContext();
        var a = Guid.NewGuid();
        db.Users.Add(new User { Id = a, UserName = "a", DisplayName = "Ada" });
        await db.SaveChangesAsync();
        var handler = new ForceMatchHandler(db, new Matchmaker(db));

        var result = await handler.Handle(new ForceMatchRequest(a, Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
        Assert.Empty(db.Matches);
    }
}
