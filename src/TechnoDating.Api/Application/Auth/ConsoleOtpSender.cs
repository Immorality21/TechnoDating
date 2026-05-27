namespace TechnoDating.Api.Application.Auth;

public class ConsoleOtpSender(ILogger<ConsoleOtpSender> logger) : IOtpSender
{
    public Task SendAsync(string phoneNumber, string code, CancellationToken cancellationToken)
    {
        logger.LogInformation("[OTP] {PhoneNumber}: {Code}", phoneNumber, code);
        return Task.CompletedTask;
    }
}
