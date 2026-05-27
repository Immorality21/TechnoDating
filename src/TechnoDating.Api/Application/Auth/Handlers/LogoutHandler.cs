using MediatR;
using TechnoDating.Api.Application.Auth.Requests;

namespace TechnoDating.Api.Application.Auth.Handlers;

public class LogoutHandler(ITokenService tokens) : IRequestHandler<LogoutRequest, bool>
{
    public Task<bool> Handle(LogoutRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Task.FromResult(false);
        }
        return tokens.RevokeAsync(request.RefreshToken, cancellationToken);
    }
}
