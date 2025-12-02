using Blazored.LocalStorage;
using PersonalFinance.Web.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace PersonalFinance.Web.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly CustomAuthStateProvider _authStateProvider;

        public AuthService(
            HttpClient httpClient,
            ILocalStorageService localStorage,
            CustomAuthStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _authStateProvider = authStateProvider;
        }

        // Регистрация
        public async Task<(bool Success, string Message)> RegisterAsync(RegisterModel model)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/register", model);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<JsonElement>();

                    if (result.TryGetProperty("token", out var tokenElement) &&
                        result.TryGetProperty("user", out var userElement))
                    {
                        var token = tokenElement.GetString();
                        var user = JsonSerializer.Deserialize<User>(userElement.GetRawText());

                        if (!string.IsNullOrEmpty(token) && user != null)
                        {
                            await _authStateProvider.NotifyUserAuthentication(token, user);
                            return (true, "Регистрация успешна!");
                        }
                    }
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return (false, $"Ошибка: {response.StatusCode} - {error}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка: {ex.Message}");
            }

            return (false, "Неизвестная ошибка");
        }

        // Вход
        public async Task<(bool Success, string Message)> LoginAsync(LoginModel model)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", model);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<JsonElement>();

                    if (result.TryGetProperty("token", out var tokenElement) &&
                        result.TryGetProperty("user", out var userElement))
                    {
                        var token = tokenElement.GetString();
                        var user = JsonSerializer.Deserialize<User>(userElement.GetRawText());

                        if (!string.IsNullOrEmpty(token) && user != null)
                        {
                            await _authStateProvider.NotifyUserAuthentication(token, user);
                            return (true, "Вход успешен!");
                        }
                    }
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return (false, $"Неверный email или пароль");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка: {ex.Message}");
            }

            return (false, "Неверный email или пароль");
        }

        // Выход
        public async Task LogoutAsync()
        {
            await _authStateProvider.NotifyUserLogout();
        }

        // Получение текущего пользователя
        public async Task<User?> GetCurrentUserAsync()
        {
            try
            {
                var userJson = await _localStorage.GetItemAsync<string>("currentUser");
                if (!string.IsNullOrEmpty(userJson))
                {
                    return JsonSerializer.Deserialize<User>(userJson);
                }
            }
            catch
            {
                // Игнорируем ошибки
            }

            return null;
        }

        // Получение токена
        public async Task<string?> GetTokenAsync()
        {
            try
            {
                return await _localStorage.GetItemAsync<string>("authToken");
            }
            catch
            {
                return null;
            }
        }

        // Инициализация заголовка авторизации
        public async Task InitializeAuthHeader()
        {
            var token = await GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}