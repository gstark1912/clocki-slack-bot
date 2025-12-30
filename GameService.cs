using System;
using System.Threading.Tasks;
using ClockiSlackBot.Services;
using ClockiSlackBot.Abstractions;

namespace ClockiSlackBot
{
    public class GameService
    {
        private readonly IGameStateRepository _stateRepo;
        private readonly IGameFlowOrchestrator _flowOrchestrator;
        private readonly IWeekService _weekService;

        public GameService(IGameStateRepository stateRepo, IGameFlowOrchestrator flowOrchestrator, IWeekService weekService)
        {
            _stateRepo = stateRepo;
            _flowOrchestrator = flowOrchestrator;
            _weekService = weekService;
        }

        public async Task RunAsync()
        {
            var today = DateTime.UtcNow.Date;
            var state = await _stateRepo.LoadAsync();

            if (_weekService.ShouldStartNewWeek(state))
            {
                await _flowOrchestrator.HandleWeekStartAsync(state);
                return;
            }

            await _flowOrchestrator.HandleDailyCheckAsync(state);

            if (_weekService.IsAlertDay(today))
                await _flowOrchestrator.HandleWarningDayAsync(state);

            if (_weekService.IsFinalDay(today))
                await _flowOrchestrator.HandleWeekEndAsync(state);
        }
    }

    // Helper DTOs for JSON files
    public record Story(int Id, string Title, string Intro, string DailyFail, string RiskHigh, string Win, string Loss);
    public record StoriesDocument(System.Collections.Generic.List<Story> Stories);
    public record VacationEntry(string Email, DateTime Date);
    public record VacationsDocument(System.Collections.Generic.List<VacationEntry> Vacations);
}
