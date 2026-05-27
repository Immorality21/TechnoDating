using MediatR;
using TechnoDating.Api.Application.Auth.Requests;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Auth.Handlers;

public class RefreshTokenHandler(ITokenService tokens) : IRequestHandler<RefreshTokenRequest, AuthResponseDto?>
{
    public Task<AuthResponseDto?> Handle(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Task.FromResult<AuthResponseDto?>(null);
        }
        return tokens.RefreshAsync(request.RefreshToken, cancellationToken);
    }
}
