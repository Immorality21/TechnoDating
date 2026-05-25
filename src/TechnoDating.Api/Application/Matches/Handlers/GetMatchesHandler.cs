using MediatR;
using TechnoDating.Api.Application.Matches.Requests;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Matches.Handlers;

public class GetMatchesHandler : IRequestHandler<GetMatchesRequest, IReadOnlyList<MatchProfileDto>>
{
    public Task<IReadOnlyList<MatchProfileDto>> Handle(GetMatchesRequest request, CancellationToken cancellationToken)
    {
        IReadOnlyList<MatchProfileDto> matches =
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

        return Task.FromResult(matches);
    }
}
