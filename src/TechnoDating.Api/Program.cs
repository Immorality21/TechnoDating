using TechnoDating.Contracts;

var builder = WebApplication.CreateBuilder(args);

const string DevCorsPolicy = "TechnoDatingDev";
builder.Services.AddCors(options =>
{
    options.AddPolicy(DevCorsPolicy, policy => policy
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors(DevCorsPolicy);
}

app.MapGet("/", () => Results.Ok(new { service = "TechnoDating.Api", status = "ok" }));

app.MapGet("/api/festivals", () => TestData.Festivals)
    .WithName("GetFestivals");

app.MapGet("/api/matches", () => TestData.Matches)
    .WithName("GetMatches");

app.Run();

internal static class TestData
{
    public static readonly IReadOnlyList<FestivalDto> Festivals =
    [
        new(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "Awakenings Festival",
            new DateOnly(2026, 7, 4),
            "Spaarnwoude",
            "Recreatiegebied Spaarnwoude",
            ["Charlotte de Witte", "Amelie Lens", "Adam Beyer", "Reinier Zonneveld"],
            MatchingPeopleCount: 12),
        new(
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            "DGTL Amsterdam",
            new DateOnly(2026, 4, 3),
            "Amsterdam",
            "NDSM-Werf",
            ["Mind Against", "Anfisa Letyago", "I Hate Models", "Indira Paganotto"],
            MatchingPeopleCount: 8),
        new(
            Guid.Parse("33333333-3333-3333-3333-333333333333"),
            "Verknipt Festival",
            new DateOnly(2026, 7, 11),
            "Utrecht",
            "Strijkviertelplas",
            ["Klangkuenstler", "FJAAK", "Pawlowski", "Sara Landry"],
            MatchingPeopleCount: 5),
        new(
            Guid.Parse("44444444-4444-4444-4444-444444444444"),
            "Lente Kabinet",
            new DateOnly(2026, 5, 18),
            "Amsterdam",
            "Het Twiske",
            ["Job Jobse", "Lauren Mia", "Carista", "DJ Pierre"],
            MatchingPeopleCount: 3),
        new(
            Guid.Parse("55555555-5555-5555-5555-555555555555"),
            "Amsterdam Dance Event",
            new DateOnly(2026, 10, 14),
            "Amsterdam",
            "Citywide",
            ["Tale Of Us", "Boris Brejcha", "Honey Dijon", "Solomun"],
            MatchingPeopleCount: 27),
    ];

    public static readonly IReadOnlyList<MatchProfileDto> Matches =
    [
        new(
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            "Sofie",
            28,
            "Amsterdam",
            ["Charlotte de Witte", "Amelie Lens", "Reinier Zonneveld"],
            ["Awakenings Festival", "Amsterdam Dance Event"],
            DistanceKm: 2.4),
        new(
            Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            "Daan",
            31,
            "Utrecht",
            ["Mind Against", "Tale Of Us", "Boris Brejcha"],
            ["DGTL Amsterdam", "Amsterdam Dance Event"],
            DistanceKm: 38.1),
        new(
            Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            "Lieke",
            26,
            "Amsterdam",
            ["Anfisa Letyago", "Indira Paganotto", "I Hate Models"],
            ["DGTL Amsterdam", "Verknipt Festival", "Awakenings Festival"],
            DistanceKm: 4.7),
        new(
            Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
            "Maud",
            29,
            "Amsterdam",
            ["Honey Dijon", "Job Jobse", "Carista"],
            ["Lente Kabinet", "Amsterdam Dance Event"],
            DistanceKm: 6.0),
    ];
}
