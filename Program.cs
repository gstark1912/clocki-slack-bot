using ClockiSlackBot;
using ClockiSlackBot.Config;
using ClockiSlackBot.Services;
using ClockiSlackBot.Repositories;
using ClockiSlackBot.Abstractions;
using ClockiSlackBot.Logger;

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
var logger = new LoggerService();

using var httpClient = new HttpClient();
IClockifyService clockifyService = new ClockifyService(httpClient, gameConfig, logger);
ISlackService slackService = new SlackService(httpClient, gameConfig, logger);
IWeekService weekService = new WeekService();
IDbService dbService = new DbService();

// New refactored services
IGameStateRepository stateRepo = new GameStateRepository();
IProgressCalculator progressCalculator = new ProgressCalculator(clockifyService, weekService, dbService, gameConfig);
IGameFlowOrchestrator flowOrchestrator = new GameFlowOrchestrator(slackService, dbService, progressCalculator, stateRepo, gameConfig);

var gameService = new GameService(stateRepo, flowOrchestrator, weekService, logger);
await gameService.RunAsync();