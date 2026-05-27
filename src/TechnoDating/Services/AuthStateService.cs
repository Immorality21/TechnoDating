using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using TechnoDating.Contracts;

namespace TechnoDating.Services;

public class AuthStateService(IHttpClientFactory httpClientFactory, ILogger<AuthStateService> logger) : IAuthStateService
{
    private const string RefreshTokenKey = "tn_refresh_token";

    private string? _accessToken;
    private string? _refreshToken;
    private UserProfileDto? _currentUser;

    public bool IsAuthenticated => _accessToken is not null && _currentUser is not null;
    public string? AccessToken => _accessToken;
    public UserProfileDto? CurrentUser => _currentUser;

    public event Action? OnAuthStateChanged;

    public async Task<bool> TryRestoreFromStorageAsync(CancellationToken cancellationToken = default)
    {
        var stored = await SecureStorage.Default.GetAsync(RefreshTokenKey);
        if (string.IsNullOrEmpty(stored))
        {
            return false;
        }
        _refreshToken = stored;
        return await RefreshAsync(cancellationToken);
    }

    public async Task<bool> RequestOtpAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        var http = httpClientFactory.CreateClient("auth");
        try
        {
            var response = await http.PostAsJsonAsync("/api/auth/request-otp", new RequestOtpDto(phoneNumber), cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "RequestOtp failed");
            return false;
        }
    }

    public async Task<bool> VerifyOtpAsync(string phoneNumber, string code, CancellationToken cancellationToken = default)
    {
        var http = httpClientFactory.CreateClient("auth");
        try
        {
            var response = await http.PostAsJsonAsync("/api/auth/verify-otp", new VerifyOtpDto(phoneNumber, code), cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }
            var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>(cancellationToken: cancellationToken);
            if (auth is null)
            {
                return false;
            }
            await ApplyAuthResponseAsync(auth);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "VerifyOtp failed");
            return false;
        }
    }

    public async Task<bool> RefreshAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_refreshToken))
        {
            return false;
        }
        var http = httpClientFactory.CreateClient("auth");
        try
        {
            var response = await http.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenDto(_refreshToken), cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                await ClearAsync();
                return false;
            }
            var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>(cancellationToken: cancellationToken);
            if (auth is null)
            {
                await ClearAsync();
                return false;
            }
            await ApplyAuthResponseAsync(auth);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Refresh failed");
            await ClearAsync();
            return false;
        }
    }

    public Task SetCurrentUserAsync(UserProfileDto user)
    {
        _currentUser = user;
        OnAuthStateChanged?.Invoke();
        return Task.CompletedTask;
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(_refreshToken))
        {
            var http = httpClientFactory.CreateClient("auth");
            try
            {
                await http.PostAsJsonAsync("/api/auth/logout", new RefreshTokenDto(_refreshToken), cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Logout request failed (revoke locally regardless)");
            }
        }
        await ClearAsync();
    }

    private async Task ApplyAuthResponseAsync(AuthResponseDto auth)
    {
        _accessToken = auth.AccessToken;
        _refreshToken = auth.RefreshToken;
        _currentUser = auth.User;
        await SecureStorage.Default.SetAsync(RefreshTokenKey, auth.RefreshToken);
        OnAuthStateChanged?.Invoke();
    }

    private Task ClearAsync()
    {
        _accessToken = null;
        _refreshToken = null;
        _currentUser = null;
        SecureStorage.Default.Remove(RefreshTokenKey);
        OnAuthStateChanged?.Invoke();
        return Task.CompletedTask;
    }
}
