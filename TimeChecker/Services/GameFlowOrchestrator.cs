using System;
using System.Linq;
using System.Threading.Tasks;
using ClockiSlackBot.Config;
using ClockiSlackBot.Models;
using ClockiSlackBot.Abstractions;

namespace ClockiSlackBot.Services
{
    public class GameFlowOrchestrator : IGameFlowOrchestrator
    {
        private readonly ISlackService _slackService;
        private readonly IDbService _dbService;
        private readonly IProgressCalculator _progressCalculator;
        private readonly IGameStateRepository _stateRepo;
        private readonly IGameConfig _config;

        public GameFlowOrchestrator(ISlackService slackService, IDbService dbService, IProgressCalculator progressCalculator, IGameStateRepository stateRepo, IGameConfig config)
        {
            _slackService = slackService;
            _dbService = dbService;
            _progressCalculator = progressCalculator;
            _stateRepo = stateRepo;
            _config = config;
        }

        public async Task HandleWeekStartAsync(GameState state)
        {
            var story = _dbService.GetStory(state.CurrentStoryId);
            await _slackService.SendMessageAsync("general", story.Intro);
            state.Status = GameStatus.InProgress;
            state.LastUpdate = DateTime.UtcNow;
            await _stateRepo.SaveAsync(state);
        }

        public async Task HandleDailyCheckAsync(GameState state)
        {
            var userProgress = await _progressCalculator.CalculateDailyProgressAsync(_config.TargetEmails);
            var story = _dbService.GetStory(state.CurrentStoryId);
            
            foreach (var user in userProgress.Where(u => u.BelowTarget))
            {
                var message = $"{story.DailyFail} (tienes {user.Hours:F2}h, objetivo {_config.TargetHours}h).";
                await _slackService.SendMessageAsync(user.Email, message);
            }
            
            state.LastUpdate = DateTime.UtcNow;
            await _stateRepo.SaveAsync(state);
        }

        public async Task HandleWarningDayAsync(GameState state)
        {
            double progress = await _progressCalculator.CalculateWeeklyProgressAsync();
            if (progress < _config.WarningThreshold)
            {
                var story = _dbService.GetStory(state.CurrentStoryId);
                await _slackService.SendMessageAsync("general", story.RiskHigh);
            }
        }

        public async Task HandleWeekEndAsync(GameState state)
        {
            double progress = await _progressCalculator.CalculateWeeklyProgressAsync();
            bool success = progress >= _config.WarningThreshold;
            
            var story = _dbService.GetStory(state.CurrentStoryId);
            var outcome = success ? story.Win : story.Loss;
            await _slackService.SendMessageAsync("general", outcome);

            state.TeamStreak = success ? state.TeamStreak + 1 : 0;
            state.CurrentStoryId = (state.CurrentStoryId % _dbService.GetStoriesCount()) + 1;
            state.Status = GameStatus.NotStarted;
            state.LastUpdate = DateTime.UtcNow;
            await _stateRepo.SaveAsync(state);
        }
    }
}