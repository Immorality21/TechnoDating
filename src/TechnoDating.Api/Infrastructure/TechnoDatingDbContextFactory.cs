using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TechnoDating.Api.Infrastructure;

/// <summary>
/// Design-time factory used by <c>dotnet ef</c>. Lets migration commands build the context
/// without running the full <c>Program.cs</c> host (which validates JWT/Storage config and
/// depends on the environment). Not used at runtime. The connection string only needs to be
/// well-formed for <c>migrations add</c> — it is not opened.
/// </summary>
public class TechnoDatingDbContextFactory : IDesignTimeDbContextFactory<TechnoDatingDbContext>
{
    public TechnoDatingDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<TechnoDatingDbContext>()
            .UseNpgsql(
                "Host=localhost;Port=5432;Database=technodating;Username=technodating;Password=dev",
                npg => npg.UseNetTopologySuite())
            .Options;
        return new TechnoDatingDbContext(options);
    }
}
