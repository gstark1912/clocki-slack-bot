using System.Text.Json.Serialization;

namespace ClockiSlackBot.Models
{
    public record ClockifySummaryResponse(
        [property: JsonPropertyName("groupOne")] List<ClockifyUserGroup>? GroupOne
    );
}
