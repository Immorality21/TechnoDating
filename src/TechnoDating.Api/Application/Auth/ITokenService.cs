using TechnoDating.Api.Infrastructure.Entities;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Auth;

public interface ITokenService
{
    Task<AuthResponseDto> IssueAsync(User user, CancellationToken cancellationToken);
    Task<AuthResponseDto?> RefreshAsync(string refreshTokenPlaintext, CancellationToken cancellationToken);
    Task<bool> RevokeAsync(string refreshTokenPlaintext, CancellationToken cancellationToken);
}
