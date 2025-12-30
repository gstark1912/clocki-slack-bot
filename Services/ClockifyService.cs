using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using ClockiSlackBot.Config;
using ClockiSlackBot.Models;
using ClockiSlackBot.Abstractions;
using ClockiSlackBot.Logger;

namespace ClockiSlackBot.Services
{
    public class ClockifyService : IClockifyService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _workspaceId;
        private readonly LoggerService _logger;

        public ClockifyService(HttpClient httpClient, IGameConfig gameConfig, LoggerService logger)
        {
            _httpClient = httpClient;
            _apiKey = gameConfig.ApiKey;
            _workspaceId = gameConfig.WorkspaceId;
            _logger = logger;
        }

        public async Task<ClockifySummaryResponse?> GetDailySummaryAsync(DateTime date)
        {
            _logger.Log($"Obteniendo resumen diario de Clockify para {date:yyyy-MM-dd}");
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
                _logger.Log($"Error al obtener reporte de Clockify: {response.StatusCode}");
                Console.WriteLine($"Error fetching Clockify report: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                return null;
            }

            _logger.Log("Resumen diario obtenido exitosamente");
            return await response.Content.ReadFromJsonAsync<ClockifySummaryResponse>();
        }

        public async Task<List<ClockifyUser>> GetUsersAsync()
        {
            _logger.Log("Obteniendo usuarios de Clockify");
            var url = $"https://api.clockify.me/api/v1/workspaces/{_workspaceId}/users";
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers.Add("x-api-key", _apiKey);

            var response = await _httpClient.SendAsync(requestMessage);
            if (!response.IsSuccessStatusCode)
            {
                _logger.Log($"Error al obtener usuarios de Clockify: {response.StatusCode}");
                Console.WriteLine($"Error fetching Clockify users: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                return new List<ClockifyUser>();
            }

            _logger.Log("Usuarios obtenidos exitosamente");
            return await response.Content.ReadFromJsonAsync<List<ClockifyUser>>() ?? new List<ClockifyUser>();
        }
    }
}
