using System.Text.Json.Serialization;

namespace ClockiSlackBot.Models
{
    public record SlackUser(
        [property: JsonPropertyName("id")] string Id
    );
}
