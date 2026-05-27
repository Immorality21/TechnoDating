using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace TechnoDating.Services;

public class TechnoDatingAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly IAuthStateService _auth;

    public TechnoDatingAuthenticationStateProvider(IAuthStateService auth)
    {
        _auth = auth;
        _auth.OnAuthStateChanged += HandleAuthStateChanged;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = _auth.IsAuthenticated && _auth.CurrentUser is { } user
            ? new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
            }, authenticationType: "jwt")
            : new ClaimsIdentity();

        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }

    private void HandleAuthStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void Dispose()
    {
        _auth.OnAuthStateChanged -= HandleAuthStateChanged;
        GC.SuppressFinalize(this);
    }
}
