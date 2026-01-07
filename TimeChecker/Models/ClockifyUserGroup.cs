using System.Text.Json.Serialization;

namespace ClockiSlackBot.Models
{
    public record ClockifyUserGroup(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("_id")] string? Id, // Attempt to capture ID if available
        [property: JsonPropertyName("duration")] long Duration
    );
}
