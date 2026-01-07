namespace ClockiSlackBot.Config
{
    public interface IGameConfig
    {
        string WorkspaceId { get; }
        string[] TargetEmails { get; }
        double TargetHours { get; }
        double WarningThreshold { get; }
        string ApiKey { get; }
        string SlackBotTriggerUrl { get; }  
    }
}
