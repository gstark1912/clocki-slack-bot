using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ClockiSlackBot.Config;
using ClockiSlackBot.Abstractions;
using ClockiSlackBot.Logger;

namespace ClockiSlackBot.Services
{
    public class SlackService : ISlackService
    {
        private readonly HttpClient _httpClient;
        private readonly string _triggerUrl;
        private readonly LoggerService _logger;

        public SlackService(HttpClient httpClient, IGameConfig gameConfig, LoggerService logger)
        {
            _httpClient = httpClient;
            _triggerUrl = gameConfig.SlackBotTriggerUrl;
            _logger = logger;
        }

        public async Task SendMessageAsync(string email, string text)
        {
            _logger.Log($"Enviando mensaje a {email}");
            var payload = new
            {
                email = email,
                mensaje = text
            };

            var request = new HttpRequestMessage(HttpMethod.Post, _triggerUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                _logger.Log($"Mensaje enviado exitosamente a {email}");
                Console.WriteLine($"Message sent to {email}.");
            }
            else
            {
                _logger.Log($"Error al enviar mensaje a {email}: {response.StatusCode}");
                Console.WriteLine($"Failed to send message to {email}: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            }
        }
    }
}
