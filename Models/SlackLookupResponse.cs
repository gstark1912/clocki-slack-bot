using System.Text.Json.Serialization;

namespace ClockiSlackBot.Models
{
    public record SlackLookupResponse(
        [property: JsonPropertyName("ok")] bool Ok,
        [property: JsonPropertyName("user")] SlackUser? User,
        [property: JsonPropertyName("error")] string? Error
    );
}
