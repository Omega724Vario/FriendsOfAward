using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FriendsOfAward;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedLocalStorage _sessionStorage;
    private ClaimsPrincipal _anonymous = new(new ClaimsIdentity());
    private ClaimsPrincipal? _currentUser = null;
    private bool _isInitialized = false;

    public CustomAuthStateProvider(ProtectedLocalStorage sessionStorage)
    {
        _sessionStorage = sessionStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // Try to load from storage if not already loaded
        if (_currentUser == null)
        {
            try
            {
                var storageResult = await _sessionStorage.GetAsync<string>("currentUser");
                if (storageResult.Success && !string.IsNullOrEmpty(storageResult.Value))
                {
                    var username = storageResult.Value;
                    ClaimsIdentity identity = new([new Claim(ClaimTypes.Name, username)], "CustomAuthType");
                    _currentUser = new ClaimsPrincipal(identity);
                }
            }
            catch
            {
                // Ignored during prerendering or if JS is unavailable
            }
        }

        return new AuthenticationState(_currentUser ?? _anonymous);
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        
        // Notify state change to trigger GetAuthenticationStateAsync again
        // This allows the check to run after the circuit is established (on client)
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task Login(string username)
    {
        ClaimsIdentity identity = new([new Claim(ClaimTypes.Name, username)], "CustomAuthType");
        _currentUser = new ClaimsPrincipal(identity);

        try
        {
            await _sessionStorage.SetAsync("currentUser", username);
        }
        catch { /* Handle error */ }

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
    }

    public async Task Logout()
    {
        _currentUser = null;
        try
        {
            await _sessionStorage.DeleteAsync("currentUser");
        }
        catch { /* Handle error */ }

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }
}
