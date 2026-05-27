using MediatR;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Auth.Requests;

public record RefreshTokenRequest(string RefreshToken) : IRequest<AuthResponseDto?>;
