namespace ClockiSlackBot.Models
{
    public class GameState
    {
        public int TeamStreak { get; set; }
        public DateTime LastUpdate { get; set; }
        public int CurrentStoryId { get; set; }
        public GameStatus Status { get; set; }

        public GameState() { }

        public GameState(int teamStreak, DateTime lastUpdate, int currentStoryId, GameStatus status = GameStatus.NotStarted)
        {
            TeamStreak = teamStreak;
            LastUpdate = lastUpdate;
            CurrentStoryId = currentStoryId;
            Status = status;
        }
    }
}
