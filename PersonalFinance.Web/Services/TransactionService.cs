using System.Net.Http.Json;
using PersonalFinance.Web.Models;

namespace PersonalFinance.Web.Services
{
    public class TransactionService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;

        public TransactionService(HttpClient httpClient, AuthService authService)
        {
            _httpClient = httpClient;
            _authService = authService;
        }

        public async Task<List<Transaction>> GetTransactionsAsync(DateTime? startDate = null, DateTime? endDate = null, int? categoryId = null)
        {
            await _authService.InitializeAuthHeader();

            var url = "api/transactions";
            var queryParams = new List<string>();

            if (startDate.HasValue)
                queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");

            if (endDate.HasValue)
                queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");

            if (categoryId.HasValue)
                queryParams.Add($"categoryId={categoryId.Value}");

            if (queryParams.Any())
                url += $"?{string.Join("&", queryParams)}";

            return await _httpClient.GetFromJsonAsync<List<Transaction>>(url)
                ?? new List<Transaction>();
        }

        public async Task<Transaction?> GetTransactionAsync(int id)
        {
            await _authService.InitializeAuthHeader();
            return await _httpClient.GetFromJsonAsync<Transaction>($"api/transactions/{id}");
        }

        public async Task<bool> CreateTransactionAsync(TransactionCreateModel transaction)
        {
            await _authService.InitializeAuthHeader();
            var response = await _httpClient.PostAsJsonAsync("api/transactions", transaction);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateTransactionAsync(int id, Transaction transaction)
        {
            await _authService.InitializeAuthHeader();
            var response = await _httpClient.PutAsJsonAsync($"api/transactions/{id}", transaction);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteTransactionAsync(int id)
        {
            await _authService.InitializeAuthHeader();
            var response = await _httpClient.DeleteAsync($"api/transactions/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<TransactionSummary?> GetSummaryAsync(DateTime startDate, DateTime endDate)
        {
            await _authService.InitializeAuthHeader();
            var url = $"api/transactions/summary?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
            return await _httpClient.GetFromJsonAsync<TransactionSummary>(url);
        }
    }
}