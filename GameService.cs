using System;
using System.IO;
using System.Net.Http;
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
        private readonly IGameConfig _config;
        private readonly string _dataDir = "data";
        private readonly string _gameStatePath;
        private readonly string _storiesPath;
        private readonly string _vacationsPath;

        public GameService(IClockifyService clockifyService, ISlackService slackService, IGameConfig config)
        {
            _clockifyService = clockifyService;
            _slackService = slackService;
            _config = config;
            _gameStatePath = Path.Combine(_dataDir, "gamestate.json");
            _storiesPath = Path.Combine(_dataDir, "stories.json");
            _vacationsPath = Path.Combine(_dataDir, "vacations.json");
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

        private Story LoadCurrentStory(int storyId)
        {
            var json = File.ReadAllText(_storiesPath);
            var doc = JsonSerializer.Deserialize<StoriesDocument>(json)!;
            return doc.Stories.Find(s => s.Id == storyId) ?? doc.Stories[0];
        }

        private bool IsVacation(DateTime date, string email)
        {
            if (!File.Exists(_vacationsPath)) return false;
            var json = File.ReadAllText(_vacationsPath);
            var vac = JsonSerializer.Deserialize<VacationsDocument>(json)!;
            foreach (var entry in vac.Vacations)
            {
                if (entry.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && entry.Date.Date == date.Date)
                    return true;
            }
            return false;
        }

        public async Task RunAsync()
        {
            var today = DateTime.UtcNow.Date;
            var state = LoadState();
            var dayOfWeek = today.DayOfWeek;

            // Determine week start (Monday)
            var weekStart = today.AddDays(-(int)dayOfWeek + (dayOfWeek == DayOfWeek.Sunday ? -6 : 1));

            // If we are on the first day of a new week, start new week
            if (state.Status == GameStatus.NotStarted && state.LastUpdate.Date < weekStart)
            {
                await StartNewWeekAsync(state);
                state = LoadState(); // reload after start
            }

            // Daily check
            await CheckDailyAsync(state);

            // Penultimate day warning (Thursday if week is Mon-Fri)
            if (dayOfWeek == DayOfWeek.Thursday)
            {
                await SendWarningAsync(state);
            }

            // End of week (Friday)
            if (dayOfWeek == DayOfWeek.Friday)
            {
                await EndWeekAsync(state);
            }
        }

        private async Task StartNewWeekAsync(GameState state)
        {
            var story = LoadCurrentStory(state.CurrentStoryId);
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
                if (IsVacation(DateTime.UtcNow, email)) continue;
                var user = users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
                if (user == null) continue;
                var entry = report?.GroupOne?.FirstOrDefault(g => g.Name.Equals(user.Name, StringComparison.OrdinalIgnoreCase));
                double hours = entry?.Duration / 3600.0 ?? 0.0;
                if (hours < _config.TargetHours)
                {
                    var story = LoadCurrentStory(state.CurrentStoryId);
                    var message = $"{story.DailyFail} (tienes {hours:F2}h, objetivo {_config.TargetHours}h).";
                    await _slackService.SendMessageAsync(email, message);
                }
            }
            state.LastUpdate = DateTime.UtcNow;
            SaveState(state);
        }

        private async Task<double> ComputeWeeklyProgressAsync()
        {
            // Week starts on Monday
            var weekStart = DateTime.UtcNow.AddDays(-(int)DateTime.UtcNow.DayOfWeek + (DateTime.UtcNow.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
            var report = await _clockifyService.GetDailySummaryAsync(weekStart);
            var users = await _clockifyService.GetUsersAsync();
            double totalSeconds = report?.GroupOne?.Sum(g => g.Duration) ?? 0;
            double targetSeconds = _config.TargetHours * 3600 * users.Count * 7; // weekly target per user
            if (targetSeconds == 0) return 0;
            return (totalSeconds / targetSeconds) * 100.0;
        }

        private async Task SendWarningAsync(GameState state)
        {
            // Compute weekly progress and send warning only if below 60%
            double progress = await ComputeWeeklyProgressAsync();
            if (progress < 60.0)
            {
                var story = LoadCurrentStory(state.CurrentStoryId);
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
            var story = LoadCurrentStory(state.CurrentStoryId);
            var outcome = success ? story.Win : story.Loss;
            await _slackService.SendMessageAsync("general", outcome);

            // Update streak
            if (success)
                state.TeamStreak += 1;
            else
                state.TeamStreak = 0;

            // advance story id
            state.CurrentStoryId = (state.CurrentStoryId % LoadStoriesCount()) + 1;
            state.Status = GameStatus.NotStarted;
            state.LastUpdate = DateTime.UtcNow;
            SaveState(state);
        }

        private int LoadStoriesCount()
        {
            var json = File.ReadAllText(_storiesPath);
            var doc = JsonSerializer.Deserialize<StoriesDocument>(json)!;
            return doc.Stories.Count;
        }
    }

    // Helper DTOs for JSON files
    public record Story(int Id, string Title, string Intro, string DailyFail, string RiskHigh, string Win, string Loss);
    public record StoriesDocument(System.Collections.Generic.List<Story> Stories);
    public record VacationEntry(string Email, DateTime Date);
    public record VacationsDocument(System.Collections.Generic.List<VacationEntry> Vacations);
}
