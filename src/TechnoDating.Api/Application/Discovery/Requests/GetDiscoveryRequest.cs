using MediatR;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Discovery.Requests;

public record GetDiscoveryRequest(Guid CurrentUserId) : IRequest<IReadOnlyList<MatchProfileDto>>;
