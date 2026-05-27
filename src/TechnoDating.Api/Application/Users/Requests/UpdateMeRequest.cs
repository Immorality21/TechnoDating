using MediatR;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Users.Requests;

public record UpdateMeRequest(Guid UserId, UpdateProfileDto Profile) : IRequest<UserProfileDto?>;
