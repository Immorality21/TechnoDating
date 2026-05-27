using TechnoDating.Api.Infrastructure.Entities;

namespace TechnoDating.Api.Application.Auth;

public interface IOtpService
{
    Task<bool> RequestAsync(string phoneNumber, CancellationToken cancellationToken);
    Task<User?> VerifyAsync(string phoneNumber, string code, CancellationToken cancellationToken);
}
