using System.Net.Http.Json;
using PersonalFinance.Web.Models;

namespace PersonalFinance.Web.Services
{
    public class CategoryService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;

        public CategoryService(HttpClient httpClient, AuthService authService)
        {
            _httpClient = httpClient;
            _authService = authService;
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            await _authService.InitializeAuthHeader();
            return await _httpClient.GetFromJsonAsync<List<Category>>("api/categories")
                ?? new List<Category>();
        }

        public async Task<Category?> GetCategoryAsync(int id)
        {
            await _authService.InitializeAuthHeader();
            return await _httpClient.GetFromJsonAsync<Category>($"api/categories/{id}");
        }

        public async Task<bool> CreateCategoryAsync(CategoryCreateModel category)
        {
            await _authService.InitializeAuthHeader();
            var response = await _httpClient.PostAsJsonAsync("api/categories", category);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateCategoryAsync(int id, Category category)
        {
            await _authService.InitializeAuthHeader();
            var response = await _httpClient.PutAsJsonAsync($"api/categories/{id}", category);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            await _authService.InitializeAuthHeader();
            var response = await _httpClient.DeleteAsync($"api/categories/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}