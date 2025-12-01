using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ClockiSlackBot
{
    public class ClockifyService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public ClockifyService(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
        }

        public async Task<ClockifySummaryResponse?> GetDailySummaryAsync(string workspaceId, DateTime date)
        {
            var dateStr = date.ToString("yyyy-MM-dd");
            var url = $"https://reports.api.clockify.me/v1/workspaces/{workspaceId}/reports/summary";

            var summaryRequest = new
            {
                dateRangeStart = $"{dateStr}T00:00:00.000Z",
                dateRangeEnd = $"{dateStr}T23:59:59.999Z",
                summaryFilter = new { groups = new[] { "USER" } }
            };

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonSerializer.Serialize(summaryRequest), Encoding.UTF8, "application/json")
            };
            requestMessage.Headers.Add("X-Api-Key", _apiKey);

            var response = await _httpClient.SendAsync(requestMessage);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error fetching Clockify report: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                return null;
            }

            return await response.Content.ReadFromJsonAsync<ClockifySummaryResponse>();
        }

        public async Task<List<ClockifyUser>> GetUsersAsync(string workspaceId)
        {
            var url = $"https://api.clockify.me/api/v1/workspaces/{workspaceId}/users";
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers.Add("x-api-key", _apiKey);

            var response = await _httpClient.SendAsync(requestMessage);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error fetching Clockify users: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                return new List<ClockifyUser>();
            }

            return await response.Content.ReadFromJsonAsync<List<ClockifyUser>>() ?? new List<ClockifyUser>();
        }
    }
}
