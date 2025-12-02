using System.Security.Claims;
using Blazored.LocalStorage;
using PersonalFinance.Web.Models;

namespace PersonalFinance.Web.Services
{
    public class CustomAuthStateProvider : Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly AuthService _authService;

        public CustomAuthStateProvider(ILocalStorageService localStorage, AuthService authService)
        {
            _localStorage = localStorage;
            _authService = authService;
        }

        public override async Task<Microsoft.AspNetCore.Components.Authorization.AuthenticationState> GetAuthenticationStateAsync()
        {
            var user = await _authService.GetCurrentUserAsync();

            if (user != null)
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
                };

                var identity = new ClaimsIdentity(claims, "Bearer");
                var principal = new ClaimsPrincipal(identity);
                return new Microsoft.AspNetCore.Components.Authorization.AuthenticationState(principal);
            }

            return new Microsoft.AspNetCore.Components.Authorization.AuthenticationState(new ClaimsPrincipal());
        }

        public void NotifyUserAuthentication(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
            };

            var identity = new ClaimsIdentity(claims, "Bearer");
            var principal = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(
                new Microsoft.AspNetCore.Components.Authorization.AuthenticationState(principal)));
        }

        public void NotifyUserLogout()
        {
            NotifyAuthenticationStateChanged(Task.FromResult(
                new Microsoft.AspNetCore.Components.Authorization.AuthenticationState(new ClaimsPrincipal())));
        }
    }
}