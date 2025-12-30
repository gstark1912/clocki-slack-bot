using ClockiSlackBot.Config;

public class GameConfig : IGameConfig
{
    public GameConfig(string workspaceId, string[] targetEmails, double targetHours, string apiKey, string slackBotTriggerUrl)
    {
        WorkspaceId = workspaceId;
        TargetEmails = targetEmails;
        TargetHours = targetHours;
        ApiKey = apiKey;
        SlackBotTriggerUrl = slackBotTriggerUrl;
    }
    
    public string ApiKey { get; }
    public string WorkspaceId { get; }
    public string[] TargetEmails { get; }
    public double TargetHours { get; }
    public string SlackBotTriggerUrl { get; }
}