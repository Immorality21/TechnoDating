namespace TechnoDating.Api.Application.Auth;

public interface IOtpSender
{
    Task SendAsync(string phoneNumber, string code, CancellationToken cancellationToken);
}
