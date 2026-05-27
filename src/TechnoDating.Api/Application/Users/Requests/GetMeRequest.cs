using MediatR;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Users.Requests;

public record GetMeRequest(Guid UserId) : IRequest<UserProfileDto?>;
