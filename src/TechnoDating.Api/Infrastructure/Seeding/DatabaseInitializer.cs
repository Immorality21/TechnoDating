using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using TechnoDating.Api.Infrastructure.Entities;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Infrastructure.Seeding;

public class DatabaseInitializer(IServiceProvider services, ILogger<DatabaseInitializer> logger) : IHostedService
{
    private static readonly GeometryFactory Geo = new(new PrecisionModel(), 4326);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TechnoDatingDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        logger.LogInformation("Applying database migrations...");
        await db.Database.MigrateAsync(cancellationToken);

        if (await db.Festivals.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Database already seeded, skipping.");
            return;
        }

        logger.LogInformation("Seeding test data...");

        var now = DateTimeOffset.UtcNow;

        // ---- Artists ----
        var artistCatalog = new (string Name, string Genre)[]
        {
            ("Charlotte de Witte",  "hard techno"),
            ("Amelie Lens",         "hard techno"),
            ("I Hate Models",       "hard techno"),
            ("Sara Landry",         "hard techno"),
            ("Klangkuenstler",      "hard techno"),
            ("FJAAK",               "hard techno"),
            ("Indira Paganotto",    "hard techno"),
            ("Anfisa Letyago",      "hard techno"),
            ("Helena Hauff",        "hard techno"),
            ("Pawlowski",           "hard techno"),
            ("Mind Against",        "melodic techno"),
            ("Tale Of Us",          "melodic techno"),
            ("Adam Beyer",          "melodic techno"),
            ("Reinier Zonneveld",   "melodic techno"),
            ("Maceo Plex",          "melodic techno"),
            ("Boris Brejcha",       "melodic techno"),
            ("Surgeon",             "industrial techno"),
            ("Marcel Dettmann",     "industrial techno"),
            ("Ben Klock",           "industrial techno"),
            ("Nina Kraviz",         "industrial techno"),
            ("Honey Dijon",         "house"),
            ("Carista",             "house"),
            ("Job Jobse",           "house"),
            ("Lauren Mia",          "house"),
            ("DJ Pierre",           "house"),
            ("Solomun",             "house"),
            ("Peggy Gou",           "house"),
            ("Avalon Emerson",      "house"),
            ("DJ Tennis",           "house"),
            ("KI/KI",               "house"),
        };

        var artistsByName = new Dictionary<string, Artist>(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, genre) in artistCatalog)
        {
            var artist = new Artist
            {
                Id = Guid.NewGuid(),
                Name = name,
                Slug = Slugify(name),
                Genre = genre,
                CreatedAt = now,
            };
            artistsByName[name] = artist;
            db.Artists.Add(artist);
        }
        await db.SaveChangesAsync(cancellationToken);

        // ---- Festivals ----
        var festivalSeed = new (Guid Id, string Name, DateOnly Date, string City, string Venue, Point Location, string[] Headliners)[]
        {
            (Guid.Parse("11111111-1111-1111-1111-111111111111"), "Awakenings Festival", new DateOnly(2026, 7, 4), "Spaarnwoude", "Recreatiegebied Spaarnwoude", Point(4.78, 52.41),
                ["Charlotte de Witte", "Amelie Lens", "Adam Beyer", "Reinier Zonneveld"]),
            (Guid.Parse("22222222-2222-2222-2222-222222222222"), "DGTL Amsterdam", new DateOnly(2026, 4, 3), "Amsterdam", "NDSM-Werf", Point(4.89, 52.40),
                ["Mind Against", "Anfisa Letyago", "I Hate Models", "Indira Paganotto"]),
            (Guid.Parse("33333333-3333-3333-3333-333333333333"), "Verknipt Festival", new DateOnly(2026, 7, 11), "Utrecht", "Strijkviertelplas", Point(5.06, 52.07),
                ["Klangkuenstler", "FJAAK", "Pawlowski", "Sara Landry"]),
            (Guid.Parse("44444444-4444-4444-4444-444444444444"), "Lente Kabinet", new DateOnly(2026, 5, 18), "Amsterdam", "Het Twiske", Point(4.93, 52.42),
                ["Job Jobse", "Lauren Mia", "Carista", "DJ Pierre"]),
            (Guid.Parse("55555555-5555-5555-5555-555555555555"), "Amsterdam Dance Event", new DateOnly(2026, 10, 14), "Amsterdam", "Citywide", Point(4.90, 52.37),
                ["Tale Of Us", "Boris Brejcha", "Honey Dijon", "Solomun"]),
        };

        foreach (var f in festivalSeed)
        {
            db.Festivals.Add(new Festival
            {
                Id = f.Id,
                Name = f.Name,
                Date = f.Date,
                City = f.City,
                Venue = f.Venue,
                Location = f.Location,
            });
            var order = 0;
            foreach (var headliner in f.Headliners)
            {
                db.FestivalHeadlineArtists.Add(new FestivalHeadlineArtist
                {
                    Id = Guid.NewGuid(),
                    FestivalId = f.Id,
                    ArtistId = artistsByName[headliner].Id,
                    BillingOrder = order++,
                });
            }
        }
        await db.SaveChangesAsync(cancellationToken);

        // ---- Users + top artists ----
        var seedUsers = new (Guid Id, string Phone, string DisplayName, DateOnly Dob, string Gender, string Bio, string City, Point Location, string[] TopArtists, bool Verified)[]
        {
            (Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "+31600000001", "Sofie", new DateOnly(1997, 4, 12), "female", "Industrial techno and long sets.", "Amsterdam", Point(4.90, 52.37),
                ["Charlotte de Witte", "Amelie Lens", "Reinier Zonneveld"], true),
            (Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "+31600000002", "Daan", new DateOnly(1994, 11, 3), "male", "Melodic techno fan, ADE regular.", "Utrecht", Point(5.12, 52.09),
                ["Mind Against", "Tale Of Us", "Boris Brejcha"], true),
            (Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), "+31600000003", "Lieke", new DateOnly(1999, 2, 28), "female", "Hard techno, no small talk.", "Amsterdam", Point(4.92, 52.36),
                ["Anfisa Letyago", "Indira Paganotto", "I Hate Models"], false),
            (Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), "+31600000004", "Maud", new DateOnly(1996, 8, 21), "female", "House over techno, always dancing.", "Amsterdam", Point(4.88, 52.38),
                ["Honey Dijon", "Job Jobse", "Carista"], true),
        };

        foreach (var seed in seedUsers)
        {
            var user = new User
            {
                Id = seed.Id,
                UserName = seed.Phone,
                PhoneNumber = seed.Phone,
                PhoneNumberConfirmed = true,
                DisplayName = seed.DisplayName,
                DateOfBirth = seed.Dob,
                Gender = seed.Gender,
                Bio = seed.Bio,
                City = seed.City,
                Location = seed.Location,
                IsVerified = seed.Verified,
                CreatedAt = now,
                LastActiveAt = now,
            };

            var result = await userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                logger.LogError("Failed to seed user {Phone}: {Errors}", seed.Phone, string.Join(", ", result.Errors.Select(e => e.Description)));
                continue;
            }

            var rank = 1;
            foreach (var artistName in seed.TopArtists)
            {
                db.UserTopArtists.Add(new UserTopArtist
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    ArtistId = artistsByName[artistName].Id,
                    Rank = rank++,
                });
            }
        }
        await db.SaveChangesAsync(cancellationToken);

        // ---- Attendance ----
        var fAwakenings = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var fDgtl = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var fVerknipt = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var fLenteKabinet = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var fAde = Guid.Parse("55555555-5555-5555-5555-555555555555");

        var sofieId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var daanId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var liekeId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        var maudId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

        var attendance = new (Guid UserId, Guid FestivalId, AttendanceStatus Status)[]
        {
            (sofieId, fAwakenings, AttendanceStatus.Going),
            (sofieId, fVerknipt, AttendanceStatus.Ticketed),
            (sofieId, fLenteKabinet, AttendanceStatus.Going),
            (sofieId, fAde, AttendanceStatus.Going),
            (daanId, fDgtl, AttendanceStatus.Ticketed),
            (daanId, fVerknipt, AttendanceStatus.Going),
            (daanId, fAde, AttendanceStatus.Going),
            (liekeId, fAwakenings, AttendanceStatus.Going),
            (liekeId, fVerknipt, AttendanceStatus.Going),
            (liekeId, fLenteKabinet, AttendanceStatus.Going),
            (maudId, fLenteKabinet, AttendanceStatus.Ticketed),
            (maudId, fAde, AttendanceStatus.Going),
        };

        foreach (var (userId, festivalId, status) in attendance)
        {
            db.Attendances.Add(new UserFestivalAttendance
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FestivalId = festivalId,
                Status = status,
                CreatedAt = now,
                UpdatedAt = now,
            });
        }
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Seed complete: {Artists} artists, {Festivals} festivals, {Users} users, {Attendances} attendances.",
            artistCatalog.Length, festivalSeed.Length, seedUsers.Length, attendance.Length);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private static Point Point(double longitude, double latitude)
    {
        return Geo.CreatePoint(new Coordinate(longitude, latitude));
    }

    private static string Slugify(string name)
    {
        var lower = name.ToLowerInvariant();
        var chars = lower.Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray();
        var collapsed = new string(chars);
        while (collapsed.Contains("--"))
        {
            collapsed = collapsed.Replace("--", "-");
        }
        return collapsed.Trim('-');
    }
}
