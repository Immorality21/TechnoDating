using TechnoDating.Contracts;

namespace TechnoDating.Services;

public interface IAuthStateService
{
    bool IsAuthenticated { get; }
    string? AccessToken { get; }
    UserProfileDto? CurrentUser { get; }

    event Action? OnAuthStateChanged;

    Task<bool> TryRestoreFromStorageAsync(CancellationToken cancellationToken = default);
    Task<bool> RequestOtpAsync(string phoneNumber, CancellationToken cancellationToken = default);
    Task<bool> VerifyOtpAsync(string phoneNumber, string code, CancellationToken cancellationToken = default);
    Task<bool> RefreshAsync(CancellationToken cancellationToken = default);
    Task SetCurrentUserAsync(UserProfileDto user);
    Task LogoutAsync(CancellationToken cancellationToken = default);
}
