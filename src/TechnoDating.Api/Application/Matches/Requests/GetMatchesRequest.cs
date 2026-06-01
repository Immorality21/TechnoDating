using MediatR;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Matches.Requests;

public record GetMatchesRequest(Guid CurrentUserId) : IRequest<IReadOnlyList<MatchDto>>;
