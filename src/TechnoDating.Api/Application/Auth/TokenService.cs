using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TechnoDating.Api.Application.Users;
using TechnoDating.Api.Infrastructure;
using TechnoDating.Api.Infrastructure.Entities;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Auth;

public class TokenService(
    TechnoDatingDbContext db,
    UserManager<User> userManager,
    IOptions<JwtOptions> jwtOptions,
    ILogger<TokenService> logger) : ITokenService
{
    private readonly JwtOptions _jwt = jwtOptions.Value;

    public async Task<AuthResponseDto> IssueAsync(User user, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var accessExpires = now.AddMinutes(_jwt.AccessTokenMinutes);
        var refreshExpires = now.AddDays(_jwt.RefreshTokenDays);

        var securityStamp = await userManager.GetSecurityStampAsync(user);
        var accessToken = BuildAccessToken(user, securityStamp, now, accessExpires);

        var refreshPlaintext = GenerateRefreshToken();
        var refreshHash = HashRefreshToken(refreshPlaintext);

        db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = refreshHash,
            IssuedAt = now,
            ExpiresAt = refreshExpires,
        });
        await db.SaveChangesAsync(cancellationToken);

        var topArtists = await db.LoadTopArtistsAsync(user.Id, cancellationToken);
        return new AuthResponseDto(
            accessToken,
            refreshPlaintext,
            accessExpires,
            user.ToProfileDto(topArtists));
    }

    public async Task<AuthResponseDto?> RefreshAsync(string refreshTokenPlaintext, CancellationToken cancellationToken)
    {
        var hash = HashRefreshToken(refreshTokenPlaintext);
        var now = DateTimeOffset.UtcNow;

        var existing = await db.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.TokenHash == hash, cancellationToken);

        if (existing is null || existing.RevokedAt != null || existing.ExpiresAt <= now)
        {
            logger.LogInformation("Refresh denied: missing/revoked/expired token");
            return null;
        }

        if (existing.User is null)
        {
            logger.LogWarning("Refresh token {TokenId} has no associated user", existing.Id);
            return null;
        }

        existing.RevokedAt = now;

        var newPair = await IssueAsync(existing.User, cancellationToken);

        var newHash = HashRefreshToken(newPair.RefreshToken);
        var newRow = await db.RefreshTokens.FirstAsync(r => r.TokenHash == newHash, cancellationToken);
        existing.ReplacedByTokenId = newRow.Id;
        await db.SaveChangesAsync(cancellationToken);

        return newPair;
    }

    public async Task<bool> RevokeAsync(string refreshTokenPlaintext, CancellationToken cancellationToken)
    {
        var hash = HashRefreshToken(refreshTokenPlaintext);
        var existing = await db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == hash, cancellationToken);

        if (existing is null || existing.RevokedAt != null)
        {
            return false;
        }

        existing.RevokedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private string BuildAccessToken(User user, string securityStamp, DateTimeOffset issuedAt, DateTimeOffset expiresAt)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("security_stamp", securityStamp),
        };
        if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
        {
            claims.Add(new Claim("phone_number", user.PhoneNumber));
        }

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            notBefore: issuedAt.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        Span<byte> buf = stackalloc byte[32];
        RandomNumberGenerator.Fill(buf);
        return Convert.ToBase64String(buf)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string HashRefreshToken(string plaintext)
    {
        var bytes = Encoding.UTF8.GetBytes(plaintext);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
