using MediatR;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Festivals.Requests;

public record GetFestivalsRequest(Guid CurrentUserId) : IRequest<IReadOnlyList<FestivalDto>>;
