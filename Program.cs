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
    
    var warningThresholdStr = Environment.GetEnvironmentVariable("WARNING_THRESHOLD") ?? "60.0";
    if (!double.TryParse(warningThresholdStr, out double warningThreshold))
        throw new ArgumentException("Invalid WARNING_THRESHOLD value", "WARNING_THRESHOLD");

    return new GameConfig(workspaceId, targetEmails.Split(','), targetHours, warningThreshold, clockifyApiKey, slackBotTriggerUrl);
}

var gameConfig = CreateGameConfig();

using var httpClient = new HttpClient();
IClockifyService clockifyService = new ClockifyService(httpClient, gameConfig);
ISlackService slackService = new SlackService(httpClient, gameConfig);
IWeekService weekService = new WeekService();
IDbService dbService = new DbService();

var gameService = new GameService(clockifyService, slackService, weekService, dbService, gameConfig);
await gameService.RunAsync();