using Microsoft.EntityFrameworkCore;
using TechnoDating.Api.Application.Admin.Handlers;
using TechnoDating.Api.Application.Admin.Requests;
using TechnoDating.Api.Infrastructure.Entities;
using TechnoDating.Contracts;
using Xunit;

namespace TechnoDating.Api.Tests;

public class CreateFestivalHandlerTests
{
    [Fact]
    public async Task CreatesFestival_WithOrderedHeadliners_IgnoringUnknownArtistIds()
    {
        using var db = TestDb.NewContext();
        var a1 = new Artist { Id = Guid.NewGuid(), Name = "Charlotte de Witte", Slug = "cdw" };
        var a2 = new Artist { Id = Guid.NewGuid(), Name = "Amelie Lens", Slug = "al" };
        db.Artists.AddRange(a1, a2);
        await db.SaveChangesAsync();

        var handler = new CreateFestivalHandler(db);
        var body = new SaveFestivalDto(
            "Awakenings",
            new DateOnly(2026, 7, 4),
            "Spaarnwoude",
            "Recreatiegebied",
            new[] { a2.Id, Guid.NewGuid(), a1.Id }); // middle id is unknown → dropped

        var result = await handler.Handle(new CreateFestivalRequest(body), CancellationToken.None);

        Assert.Equal("Awakenings", result.Name);
        Assert.Equal(new[] { a2.Id, a1.Id }, result.Headliners.Select(h => h.Id).ToArray());
        Assert.Single(db.Festivals);
        Assert.Equal(2, await db.FestivalHeadlineArtists.CountAsync());
    }
}
