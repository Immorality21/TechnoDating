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

        db.Festivals.AddRange(festivals);
        await db.SaveChangesAsync(cancellationToken);

        var seedUsers = new (Guid Id, string Phone, string DisplayName, DateOnly Dob, string Gender, string Bio, string City, Point Location, List<string> TopArtists, bool Verified)[]
        {
            (Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "+31600000001", "Sofie", new DateOnly(1997, 4, 12), "female", "Industrial techno and long sets.", "Amsterdam", Point(4.90, 52.37), ["Charlotte de Witte", "Amelie Lens", "Reinier Zonneveld"], true),
            (Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "+31600000002", "Daan", new DateOnly(1994, 11, 3), "male", "Melodic techno fan, ADE regular.", "Utrecht", Point(5.12, 52.09), ["Mind Against", "Tale Of Us", "Boris Brejcha"], true),
            (Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), "+31600000003", "Lieke", new DateOnly(1999, 2, 28), "female", "Hard techno, no small talk.", "Amsterdam", Point(4.92, 52.36), ["Anfisa Letyago", "Indira Paganotto", "I Hate Models"], false),
            (Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), "+31600000004", "Maud", new DateOnly(1996, 8, 21), "female", "House over techno, always dancing.", "Amsterdam", Point(4.88, 52.38), ["Honey Dijon", "Job Jobse", "Carista"], true),
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
                TopArtists = seed.TopArtists,
                IsVerified = seed.Verified,
                CreatedAt = now,
                LastActiveAt = now,
            };

            var result = await userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                logger.LogError("Failed to seed user {Phone}: {Errors}", seed.Phone, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

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

        logger.LogInformation("Seed complete: {Festivals} festivals, {Users} users, {Attendances} attendances.", festivals.Count, seedUsers.Length, attendance.Length);
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
