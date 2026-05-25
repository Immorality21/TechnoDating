using MediatR;
using TechnoDating.Api.Application.Festivals.Requests;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Festivals.Handlers;

public class GetFestivalsHandler : IRequestHandler<GetFestivalsRequest, IReadOnlyList<FestivalDto>>
{
    public Task<IReadOnlyList<FestivalDto>> Handle(GetFestivalsRequest request, CancellationToken cancellationToken)
    {
        IReadOnlyList<FestivalDto> festivals =
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

        return Task.FromResult(festivals);
    }
}
