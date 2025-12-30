using System;
using System.Threading.Tasks;
using ClockiSlackBot.Services;
using ClockiSlackBot.Abstractions;
using ClockiSlackBot.Logger;

namespace ClockiSlackBot
{
    public class GameService
    {
        private readonly IGameStateRepository _stateRepo;
        private readonly IGameFlowOrchestrator _flowOrchestrator;
        private readonly IWeekService _weekService;
        private readonly LoggerService _logger;

        public GameService(IGameStateRepository stateRepo, IGameFlowOrchestrator flowOrchestrator, IWeekService weekService, LoggerService logger)
        {
            _stateRepo = stateRepo;
            _flowOrchestrator = flowOrchestrator;
            _weekService = weekService;
            _logger = logger;
        }

        public async Task RunAsync()
        {
            _logger.Log("Iniciando GameService");
            var today = DateTime.UtcNow.Date;
            var state = await _stateRepo.LoadAsync();

            if (_weekService.ShouldStartNewWeek(state))
            {
                _logger.Log("Iniciando nueva semana");
                await _flowOrchestrator.HandleWeekStartAsync(state);
                return;
            }

            _logger.Log("Ejecutando chequeo diario");
            await _flowOrchestrator.HandleDailyCheckAsync(state);

            if (_weekService.IsAlertDay(today))
            {
                _logger.Log("Día de alerta - enviando advertencias");
                await _flowOrchestrator.HandleWarningDayAsync(state);
            }

            if (_weekService.IsFinalDay(today))
            {
                _logger.Log("Día final de la semana");
                await _flowOrchestrator.HandleWeekEndAsync(state);
            }
            
            _logger.Log("GameService completado");
        }
    }

    // Helper DTOs for JSON files
    public record Story(int Id, string Title, string Intro, string DailyFail, string RiskHigh, string Win, string Loss);
    public record StoriesDocument(System.Collections.Generic.List<Story> Stories);
    public record VacationEntry(string Email, DateTime Date);
    public record VacationsDocument(System.Collections.Generic.List<VacationEntry> Vacations);
}
