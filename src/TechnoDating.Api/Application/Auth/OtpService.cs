using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TechnoDating.Api.Infrastructure;
using TechnoDating.Api.Infrastructure.Entities;

namespace TechnoDating.Api.Application.Auth;

public class OtpService(
    TechnoDatingDbContext db,
    UserManager<User> userManager,
    IOtpSender sender,
    IPasswordHasher<OtpChallenge> codeHasher,
    IOptions<OtpOptions> options,
    ILogger<OtpService> logger) : IOtpService
{
    private readonly OtpOptions _opts = options.Value;

    public async Task<bool> RequestAsync(string phoneNumber, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var cooldownThreshold = now.AddSeconds(-_opts.ResendCooldownSeconds);

        var hasRecent = await db.OtpChallenges
            .AsNoTracking()
            .Where(o => o.PhoneNumber == phoneNumber && o.CreatedAt > cooldownThreshold)
            .AnyAsync(cancellationToken);

        if (hasRecent)
        {
            logger.LogInformation("OTP request denied (cooldown) for {PhoneNumber}", phoneNumber);
            return false;
        }

        var code = GenerateCode(_opts.CodeLength);
        var challenge = new OtpChallenge
        {
            Id = Guid.NewGuid(),
            PhoneNumber = phoneNumber,
            ExpiresAt = now.AddMinutes(_opts.LifetimeMinutes),
            CreatedAt = now,
        };
        challenge.CodeHash = codeHasher.HashPassword(challenge, code);

        db.OtpChallenges.Add(challenge);
        await db.SaveChangesAsync(cancellationToken);

        await sender.SendAsync(phoneNumber, code, cancellationToken);
        return true;
    }

    public async Task<User?> VerifyAsync(string phoneNumber, string code, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        var challenge = await db.OtpChallenges
            .Where(o =>
                o.PhoneNumber == phoneNumber
                && o.ConsumedAt == null
                && o.ExpiresAt > now
                && o.AttemptCount < _opts.MaxAttempts)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (challenge is null)
        {
            logger.LogInformation("OTP verify: no active challenge for {PhoneNumber}", phoneNumber);
            return null;
        }

        var verification = codeHasher.VerifyHashedPassword(challenge, challenge.CodeHash, code);
        if (verification == PasswordVerificationResult.Failed)
        {
            challenge.AttemptCount++;
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("OTP verify: wrong code for {PhoneNumber} (attempt {Attempt})", phoneNumber, challenge.AttemptCount);
            return null;
        }

        challenge.ConsumedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        var user = await userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber, cancellationToken);
        if (user is null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                UserName = phoneNumber,
                PhoneNumber = phoneNumber,
                PhoneNumberConfirmed = true,
                CreatedAt = now,
                LastActiveAt = now,
            };

            var create = await userManager.CreateAsync(user);
            if (!create.Succeeded)
            {
                logger.LogError("Failed to create user for {PhoneNumber}: {Errors}", phoneNumber, string.Join(", ", create.Errors.Select(e => e.Description)));
                return null;
            }
            logger.LogInformation("Created new user {UserId} for {PhoneNumber}", user.Id, phoneNumber);
        }
        else
        {
            user.LastActiveAt = now;
            user.PhoneNumberConfirmed = true;
            await db.SaveChangesAsync(cancellationToken);
        }

        return user;
    }

    private static string GenerateCode(int length)
    {
        var max = (int)Math.Pow(10, length);
        var n = RandomNumberGenerator.GetInt32(0, max);
        return n.ToString(new string('0', length));
    }
}
