using TechnoDating.Api.Application.Matches;
using TechnoDating.Contracts;
using Xunit;

namespace TechnoDating.Api.Tests;

public class MatchmakerTests
{
    [Fact]
    public async Task CreatesMatch_WithCanonicalPairOrdering()
    {
        using var db = TestDb.NewContext();
        var mm = new Matchmaker(db);
        var a = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var b = Guid.Parse("00000000-0000-0000-0000-000000000002");

        // Pass the pair reversed — the matchmaker must canonicalize to (low, high).
        var match = await mm.TryCreateMatchAsync(b, a, MatchOrigin.MutualLike, CancellationToken.None);

        Assert.Equal(a, match.UserAId);
        Assert.Equal(b, match.UserBId);
        Assert.Equal(MatchOrigin.MutualLike, match.Origin);
        Assert.Equal(MatchStatus.Active, match.Status);
    }

    [Fact]
    public async Task IsIdempotent_RegardlessOfArgumentOrder()
    {
        using var db = TestDb.NewContext();
        var mm = new Matchmaker(db);
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();

        var first = await mm.TryCreateMatchAsync(a, b, MatchOrigin.MutualLike, CancellationToken.None);
        var second = await mm.TryCreateMatchAsync(b, a, MatchOrigin.Curated, CancellationToken.None);

        Assert.Equal(first.Id, second.Id);
        Assert.Single(db.Matches);
        // The existing match wins — a second call does not overwrite the original origin.
        Assert.Equal(MatchOrigin.MutualLike, second.Origin);
    }

    [Fact]
    public async Task Throws_WhenMatchingUserWithThemselves()
    {
        using var db = TestDb.NewContext();
        var mm = new Matchmaker(db);
        var a = Guid.NewGuid();

        await Assert.ThrowsAsync<ArgumentException>(
            () => mm.TryCreateMatchAsync(a, a, MatchOrigin.MutualLike, CancellationToken.None));
    }
}
