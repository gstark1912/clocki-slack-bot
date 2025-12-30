using ClockiSlackBot;
using ClockiSlackBot.Config;
using ClockiSlackBot.Services;

static IGameConfig CreateGameConfig()
{
    var clockifyApiKey = Environment.GetEnvironmentVariable("CLOCKIFY_API_KEY")
        ?? throw new ArgumentNullException("CLOCKIFY_API_KEY");
    var workspaceId = Environment.GetEnvironmentVariable("CLOCKIFY_WORKSPACE_ID")
        ?? throw new ArgumentNullException("CLOCKIFY_WORKSPACE_ID");
    var slackBotTriggerUrl = Environment.GetEnvironmentVariable("SLACK_BOT_TRIGGER_URL")
        ?? throw new ArgumentNullException("SLACK_BOT_TRIGGER_URL");
    var targetEmails = Environment.GetEnvironmentVariable("TARGET_EMAILS")
        ?? throw new ArgumentNullException("TARGET_EMAILS");
    var targetHoursStr = Environment.GetEnvironmentVariable("TARGET_HOURS")
        ?? throw new ArgumentNullException("TARGET_HOURS");
    if (!double.TryParse(targetHoursStr, out double targetHours))
        throw new ArgumentException("Invalid TARGET_HOURS value", "TARGET_HOURS");

    return new GameConfig(workspaceId, targetEmails.Split(','), targetHours, clockifyApiKey, slackBotTriggerUrl);
}

var gameConfig = CreateGameConfig();

using var httpClient = new HttpClient();
IClockifyService clockifyService = new ClockifyService(httpClient, gameConfig);
ISlackService slackService = new SlackService(httpClient, gameConfig);

var gameService = new GameService(clockifyService, slackService, gameConfig);
await gameService.RunAsync();