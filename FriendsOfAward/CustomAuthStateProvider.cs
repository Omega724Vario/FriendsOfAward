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

    private static readonly TimeSpan SessionDuration = TimeSpan.FromMinutes(30);

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
                var storageResult = await _sessionStorage.GetAsync<AuthSession>("authSession");
                if (storageResult.Success && storageResult.Value != null)
                {
                    var session = storageResult.Value;
                    
                    // Check if session has expired
                    if (DateTime.UtcNow <= session.ExpiresAt)
                    {
                        ClaimsIdentity identity = new([new Claim(ClaimTypes.Name, session.Username)], "CustomAuthType");
                        _currentUser = new ClaimsPrincipal(identity);
                    }
                    else
                    {
                        // Session expired, clear it
                        await _sessionStorage.DeleteAsync("authSession");
                    }
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
            var session = new AuthSession
            {
                Username = username,
                ExpiresAt = DateTime.UtcNow.Add(SessionDuration)
            };
            await _sessionStorage.SetAsync("authSession", session);
        }
        catch { /* Handle error */ }

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
    }

    public async Task Logout()
    {
        _currentUser = null;
        try
        {
            await _sessionStorage.DeleteAsync("authSession");
        }
        catch { /* Handle error */ }

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }
    
    private class AuthSession
    {
        public string Username { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
