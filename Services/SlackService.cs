using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ClockiSlackBot.Config;

namespace ClockiSlackBot.Services
{
    public class SlackService : ISlackService
    {
        private readonly HttpClient _httpClient;
        private readonly string _triggerUrl;

        public SlackService(HttpClient httpClient, IGameConfig gameConfig)
        {
            _httpClient = httpClient;
            _triggerUrl = gameConfig.SlackBotTriggerUrl;
        }

        public async Task SendMessageAsync(string email, string text)
        {
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
                Console.WriteLine($"Message sent to {email}.");
            }
            else
            {
                Console.WriteLine($"Failed to send message to {email}: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            }
        }
    }
}
