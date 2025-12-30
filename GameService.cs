using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using ClockiSlackBot.Config;
using ClockiSlackBot.Services;
using ClockiSlackBot.Models;

namespace ClockiSlackBot
{
    public class GameService
    {
        private readonly IClockifyService _clockifyService;
        private readonly ISlackService _slackService;
        private readonly IWeekService _weekService;
        private readonly IDbService _dbService;
        private readonly IGameConfig _config;
        private readonly string _dataDir = "data";
        private readonly string _gameStatePath;

        public GameService(IClockifyService clockifyService, ISlackService slackService, IWeekService weekService, IDbService dbService, IGameConfig config)
        {
            _clockifyService = clockifyService;
            _slackService = slackService;
            _weekService = weekService;
            _dbService = dbService;
            _config = config;
            _gameStatePath = Path.Combine(_dataDir, "gamestate.json");
        }

        private GameState LoadState()
        {
            if (!File.Exists(_gameStatePath))
            {
                var init = new GameState(0, DateTime.UtcNow, 1, GameStatus.NotStarted);
                File.WriteAllText(_gameStatePath, JsonSerializer.Serialize(init, new JsonSerializerOptions { WriteIndented = true }));
                return init;
            }
            var json = File.ReadAllText(_gameStatePath);
            return JsonSerializer.Deserialize<GameState>(json)!;
        }

        private void SaveState(GameState state)
        {
            var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_gameStatePath, json);
        }

        public async Task RunAsync()
        {
            var today = DateTime.UtcNow.Date;
            var state = LoadState();

            if (_weekService.ShouldStartNewWeek(state))
            {
                await StartNewWeekAsync(state);
                state = LoadState(); // reload after start
                return;
            }

            // Daily check
            await CheckDailyAsync(state);

            // Alert day warning (Thursday)
            if (_weekService.IsAlertDay(today))
            {
                await SendWarningAsync(state);
            }

            // Final day (Friday)
            if (_weekService.IsFinalDay(today))
            {
                await EndWeekAsync(state);
            }
        }

        private async Task StartNewWeekAsync(GameState state)
        {
            var story = _dbService.GetStory(state.CurrentStoryId);
            await _slackService.SendMessageAsync("general", story.Intro);
            state.Status = GameStatus.InProgress;
            state.LastUpdate = DateTime.UtcNow;
            SaveState(state);
        }

        private async Task CheckDailyAsync(GameState state)
        {
            var targetEmails = _config.TargetEmails;
            var users = await _clockifyService.GetUsersAsync();
            var report = await _clockifyService.GetDailySummaryAsync(DateTime.UtcNow);
            foreach (var email in targetEmails)
            {
                if (_dbService.IsVacation(DateTime.UtcNow, email)) continue;
                var user = users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
                if (user == null) continue;
                var entry = report?.GroupOne?.FirstOrDefault(g => g.Name.Equals(user.Name, StringComparison.OrdinalIgnoreCase));
                double hours = entry?.Duration / 3600.0 ?? 0.0;
                if (hours < _config.TargetHours)
                {
                    var story = _dbService.GetStory(state.CurrentStoryId);
                    var message = $"{story.DailyFail} (tienes {hours:F2}h, objetivo {_config.TargetHours}h).";
                    await _slackService.SendMessageAsync(email, message);
                }
            }
            state.LastUpdate = DateTime.UtcNow;
            SaveState(state);
        }

        private async Task<double> ComputeWeeklyProgressAsync()
        {
            var weekStart = _weekService.GetWeekStart(DateTime.UtcNow);
            var report = await _clockifyService.GetDailySummaryAsync(weekStart);
            var users = await _clockifyService.GetUsersAsync();
            double totalSeconds = report?.GroupOne?.Sum(g => g.Duration) ?? 0;
            double targetSeconds = _config.TargetHours * 3600 * users.Count * 7; // weekly target per user
            if (targetSeconds == 0) return 0;
            return (totalSeconds / targetSeconds) * 100.0;
        }

        private async Task SendWarningAsync(GameState state)
        {
            // Compute weekly progress and send warning only if below threshold
            double progress = await ComputeWeeklyProgressAsync();
            if (progress < _config.WarningThreshold)
            {
                var story = _dbService.GetStory(state.CurrentStoryId);
                await _slackService.SendMessageAsync("general", story.RiskHigh);
            }
        }

        private async Task EndWeekAsync(GameState state)
        {
            // Compute overall team progress
            var users = await _clockifyService.GetUsersAsync();
            var weekStart = DateTime.UtcNow.AddDays(-6); // unchanged
            var report = await _clockifyService.GetDailySummaryAsync(weekStart); // simplistic: use first day summary as placeholder
            // In real implementation we'd aggregate week data; here we just assume success if any entry exists
            bool success = report?.GroupOne?.Count > 0; // placeholder logic
            var story = _dbService.GetStory(state.CurrentStoryId);
            var outcome = success ? story.Win : story.Loss;
            await _slackService.SendMessageAsync("general", outcome);

            // Update streak
            if (success)
                state.TeamStreak += 1;
            else
                state.TeamStreak = 0;

            // advance story id
            state.CurrentStoryId = (state.CurrentStoryId % _dbService.GetStoriesCount()) + 1;
            state.Status = GameStatus.NotStarted;
            state.LastUpdate = DateTime.UtcNow;
            SaveState(state);
        }


    }

    // Helper DTOs for JSON files
    public record Story(int Id, string Title, string Intro, string DailyFail, string RiskHigh, string Win, string Loss);
    public record StoriesDocument(System.Collections.Generic.List<Story> Stories);
    public record VacationEntry(string Email, DateTime Date);
    public record VacationsDocument(System.Collections.Generic.List<VacationEntry> Vacations);
}
