using MediatR;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Festivals.Requests;

public record GetFestivalAttendeesRequest(Guid CurrentUserId, Guid FestivalId) : IRequest<IReadOnlyList<MatchProfileDto>>;
