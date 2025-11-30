using System.Text.Json.Serialization;

namespace ClockiSlackBot
{
    public record ClockifySummaryResponse(
        [property: JsonPropertyName("groupOne")] List<ClockifyUserGroup>? GroupOne
    );

    public record ClockifyUserGroup(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("_id")] string? Id, // Attempt to capture ID if available
        [property: JsonPropertyName("duration")] long Duration
    );

    public record ClockifyUser(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("email")] string Email,
        [property: JsonPropertyName("name")] string Name
    );

    // Slack models might not be needed for lookup anymore, but keeping them won't hurt if we revert.
    public record SlackLookupResponse(
        [property: JsonPropertyName("ok")] bool Ok,
        [property: JsonPropertyName("user")] SlackUser? User,
        [property: JsonPropertyName("error")] string? Error
    );

    public record SlackUser(
        [property: JsonPropertyName("id")] string Id
    );
}
