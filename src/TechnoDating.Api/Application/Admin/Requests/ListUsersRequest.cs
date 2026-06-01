using MediatR;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Admin.Requests;

public record ListUsersRequest : IRequest<IReadOnlyList<AdminUserDto>>;
