using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using ClockiSlackBot.Config;
using ClockiSlackBot.Models;
using ClockiSlackBot.Abstractions;

namespace ClockiSlackBot.Services
{
    public class ClockifyService : IClockifyService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _workspaceId;

        public ClockifyService(HttpClient httpClient, IGameConfig gameConfig)
        {
            _httpClient = httpClient;
            _apiKey = gameConfig.ApiKey;
            _workspaceId = gameConfig.WorkspaceId;
        }

        public async Task<ClockifySummaryResponse?> GetDailySummaryAsync(DateTime date)
        {
            var dateStr = date.ToString("yyyy-MM-dd");
            var url = $"https://reports.api.clockify.me/v1/workspaces/{_workspaceId}/reports/summary";

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

        public async Task<List<ClockifyUser>> GetUsersAsync()
        {
            var url = $"https://api.clockify.me/api/v1/workspaces/{_workspaceId}/users";
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
