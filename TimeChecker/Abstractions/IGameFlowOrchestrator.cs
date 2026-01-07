using System.Threading.Tasks;
using ClockiSlackBot.Models;

namespace ClockiSlackBot.Abstractions
{
    public interface IGameFlowOrchestrator
    {
        Task HandleWeekStartAsync(GameState state);
        Task HandleDailyCheckAsync(GameState state);
        Task HandleWarningDayAsync(GameState state);
        Task HandleWeekEndAsync(GameState state);
    }
}