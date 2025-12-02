using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using PersonalFinance.Web.Models;
using System.Security.Claims;
using System.Text.Json;

namespace PersonalFinance.Web.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;

        public CustomAuthStateProvider(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // Получаем токен из Local Storage
                var token = await _localStorage.GetItemAsync<string>("authToken");

                if (!string.IsNullOrEmpty(token))
                {
                    // Получаем пользователя из Local Storage
                    var userJson = await _localStorage.GetItemAsync<string>("currentUser");

                    if (!string.IsNullOrEmpty(userJson))
                    {
                        try
                        {
                            var user = JsonSerializer.Deserialize<User>(userJson);

                            if (user != null)
                            {
                                // Создаем claims на основе пользователя
                                var claims = new List<Claim>
                                {
                                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                                    new Claim(ClaimTypes.Email, user.Email ?? ""),
                                    new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
                                };

                                var identity = new ClaimsIdentity(claims, "Bearer");
                                var principal = new ClaimsPrincipal(identity);

                                return new AuthenticationState(principal);
                            }
                        }
                        catch (JsonException)
                        {
                            // Если не удалось десериализовать пользователя
                            await _localStorage.RemoveItemAsync("currentUser");
                        }
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки Local Storage
            }

            // Если нет токена или пользователя - возвращаем пустого пользователя
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        public async Task NotifyUserAuthentication(string token, User user)
        {
            try
            {
                // Сохраняем токен и пользователя
                await _localStorage.SetItemAsync("authToken", token);
                await _localStorage.SetItemAsync("currentUser", JsonSerializer.Serialize(user));

                // Создаем claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email ?? ""),
                    new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
                };

                var identity = new ClaimsIdentity(claims, "Bearer");
                var principal = new ClaimsPrincipal(identity);

                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
            }
            catch
            {
                // Игнорируем ошибки
            }
        }

        public async Task NotifyUserLogout()
        {
            try
            {
                await _localStorage.RemoveItemAsync("authToken");
                await _localStorage.RemoveItemAsync("currentUser");
            }
            catch
            {
                // Игнорируем ошибки
            }

            NotifyAuthenticationStateChanged(Task.FromResult(
                new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
        }
    }
}