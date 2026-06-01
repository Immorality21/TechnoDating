using Microsoft.EntityFrameworkCore;
using TechnoDating.Api.Infrastructure;

namespace TechnoDating.Api.Tests;

/// <summary>
/// In-memory <see cref="TechnoDatingDbContext"/> for fast, isolated logic tests. The matching
/// code checks for existing rows before inserting, so it does not rely on DB-enforced unique
/// indexes (which the in-memory provider does not enforce). Spatial queries are not exercised here.
/// </summary>
internal static class TestDb
{
    public static TechnoDatingDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<TechnoDatingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new TechnoDatingDbContext(options);
    }
}
