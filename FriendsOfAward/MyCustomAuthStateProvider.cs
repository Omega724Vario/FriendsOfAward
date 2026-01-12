using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace FriendsOfAward
{

    public class MyCustomAuthStateProvider : AuthenticationStateProvider
    {
        private ClaimsPrincipal _anonymous = new(new ClaimsIdentity());
        // _anonymous - falls _currentUser null ist
        private ClaimsPrincipal? _currentUser = null;


        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(new AuthenticationState(_currentUser ?? _anonymous));
        }


        public void Login(string username)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, username),
        new Claim(ClaimTypes.Role, "Admin") // 🔥 DAS FEHLT
    };

            ClaimsIdentity identity = new(
                claims,
                "MyCustomAuthType");

            _currentUser = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }


        public void Logout()
        {
            _currentUser = null;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }



}
