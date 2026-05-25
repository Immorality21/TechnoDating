using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using TechnoDating.Api.Infrastructure.Entities;

namespace TechnoDating.Api.Infrastructure.Seeding;

public class DatabaseInitializer(IServiceProvider services, ILogger<DatabaseInitializer> logger) : IHostedService
{
    private static readonly GeometryFactory Geo = new(new PrecisionModel(), 4326);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TechnoDatingDbContext>();

        logger.LogInformation("Applying database migrations...");
        await db.Database.MigrateAsync(cancellationToken);

        if (await db.Festivals.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Database already seeded, skipping.");
            return;
        }

        logger.LogInformation("Seeding test data...");

        var now = DateTimeOffset.UtcNow;

        var festivals = new List<Festival>
        {
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Awakenings Festival",
                Date = new DateOnly(2026, 7, 4),
                City = "Spaarnwoude",
                Venue = "Recreatiegebied Spaarnwoude",
                HeadlineArtists = ["Charlotte de Witte", "Amelie Lens", "Adam Beyer", "Reinier Zonneveld"],
                Location = Point(4.78, 52.41),
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "DGTL Amsterdam",
                Date = new DateOnly(2026, 4, 3),
                City = "Amsterdam",
                Venue = "NDSM-Werf",
                HeadlineArtists = ["Mind Against", "Anfisa Letyago", "I Hate Models", "Indira Paganotto"],
                Location = Point(4.89, 52.40),
            },
            new()
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Verknipt Festival",
                Date = new DateOnly(2026, 7, 11),
                City = "Utrecht",
                Venue = "Strijkviertelplas",
                HeadlineArtists = ["Klangkuenstler", "FJAAK", "Pawlowski", "Sara Landry"],
                Location = Point(5.06, 52.07),
            },
            new()
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Name = "Lente Kabinet",
                Date = new DateOnly(2026, 5, 18),
                City = "Amsterdam",
                Venue = "Het Twiske",
                HeadlineArtists = ["Job Jobse", "Lauren Mia", "Carista", "DJ Pierre"],
                Location = Point(4.93, 52.42),
            },
            new()
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Name = "Amsterdam Dance Event",
                Date = new DateOnly(2026, 10, 14),
                City = "Amsterdam",
                Venue = "Citywide",
                HeadlineArtists = ["Tale Of Us", "Boris Brejcha", "Honey Dijon", "Solomun"],
                Location = Point(4.90, 52.37),
            },
        };

        var users = new List<User>
        {
            new()
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Email = "sofie@example.test",
                DisplayName = "Sofie",
                DateOfBirth = new DateOnly(1997, 4, 12),
                Gender = "female",
                Bio = "Industrial techno and long sets.",
                City = "Amsterdam",
                Location = Point(4.90, 52.37),
                TopArtists = ["Charlotte de Witte", "Amelie Lens", "Reinier Zonneveld"],
                IsVerified = true,
                CreatedAt = now,
                LastActiveAt = now,
            },
            new()
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                Email = "daan@example.test",
                DisplayName = "Daan",
                DateOfBirth = new DateOnly(1994, 11, 3),
                Gender = "male",
                Bio = "Melodic techno fan, ADE regular.",
                City = "Utrecht",
                Location = Point(5.12, 52.09),
                TopArtists = ["Mind Against", "Tale Of Us", "Boris Brejcha"],
                IsVerified = true,
                CreatedAt = now,
                LastActiveAt = now,
            },
            new()
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                Email = "lieke@example.test",
                DisplayName = "Lieke",
                DateOfBirth = new DateOnly(1999, 2, 28),
                Gender = "female",
                Bio = "Hard techno, no small talk.",
                City = "Amsterdam",
                Location = Point(4.92, 52.36),
                TopArtists = ["Anfisa Letyago", "Indira Paganotto", "I Hate Models"],
                IsVerified = false,
                CreatedAt = now,
                LastActiveAt = now,
            },
            new()
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                Email = "maud@example.test",
                DisplayName = "Maud",
                DateOfBirth = new DateOnly(1996, 8, 21),
                Gender = "female",
                Bio = "House over techno, always dancing.",
                City = "Amsterdam",
                Location = Point(4.88, 52.38),
                TopArtists = ["Honey Dijon", "Job Jobse", "Carista"],
                IsVerified = true,
                CreatedAt = now,
                LastActiveAt = now,
            },
        };

        db.Festivals.AddRange(festivals);
        db.Users.AddRange(users);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seed complete: {Festivals} festivals, {Users} users.", festivals.Count, users.Count);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private static Point Point(double longitude, double latitude)
    {
        return Geo.CreatePoint(new Coordinate(longitude, latitude));
    }
}
