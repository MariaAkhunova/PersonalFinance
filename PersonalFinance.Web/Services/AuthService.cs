using System.Net.Http.Json;
using PersonalFinance.Web.Models;
using Blazored.LocalStorage;
using System.Net.Http.Headers;

namespace PersonalFinance.Web.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private const string TokenKey = "authToken";
        private const string UserKey = "currentUser";

        public AuthService(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }

        public async Task<bool> RegisterAsync(RegisterModel model)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/register", model);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
                    if (result != null && !string.IsNullOrEmpty(result.Token))
                    {
                        await SaveAuthData(result.Token, result.User);
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> LoginAsync(LoginModel model)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", model);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
                    if (result != null && !string.IsNullOrEmpty(result.Token))
                    {
                        await SaveAuthData(result.Token, result.User);
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                await _localStorage.RemoveItemAsync(TokenKey);
                await _localStorage.RemoveItemAsync(UserKey);
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
            catch { }
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var token = await GetTokenAsync();
            return !string.IsNullOrEmpty(token);
        }

        public async Task<string?> GetTokenAsync()
        {
            try
            {
                return await _localStorage.GetItemAsync<string>(TokenKey);
            }
            catch
            {
                return null;
            }
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            try
            {
                return await _localStorage.GetItemAsync<User>(UserKey);
            }
            catch
            {
                return null;
            }
        }

        public async Task InitializeAuthHeader()
        {
            var token = await GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        private async Task SaveAuthData(string token, User user)
        {
            try
            {
                await _localStorage.SetItemAsync(TokenKey, token);
                await _localStorage.SetItemAsync(UserKey, user);
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
            catch { }
        }
    }

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public User User { get; set; } = null!;
        public string Message { get; set; } = string.Empty;
    }
}