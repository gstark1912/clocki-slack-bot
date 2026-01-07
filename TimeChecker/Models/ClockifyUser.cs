using System.Text.Json.Serialization;

namespace ClockiSlackBot.Models
{
    public record ClockifyUser(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("email")] string Email,
        [property: JsonPropertyName("name")] string Name
    );
}
