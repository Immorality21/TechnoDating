namespace TechnoDating.Api.Application.Auth;

public class OtpOptions
{
    public int CodeLength { get; set; } = 6;
    public int LifetimeMinutes { get; set; } = 5;
    public int MaxAttempts { get; set; } = 5;
    public int ResendCooldownSeconds { get; set; } = 60;
}
