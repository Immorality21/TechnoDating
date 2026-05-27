using MediatR;

namespace TechnoDating.Api.Application.Auth.Requests;

public record LogoutRequest(string RefreshToken) : IRequest<bool>;
