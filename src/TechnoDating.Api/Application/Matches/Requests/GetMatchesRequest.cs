using MediatR;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Matches.Requests;

public record GetMatchesRequest() : IRequest<IReadOnlyList<MatchProfileDto>>;
