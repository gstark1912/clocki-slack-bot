using System.Threading.Tasks;

namespace ClockiSlackBot.Abstractions
{
    public interface IProgressCalculator
    {
        Task<double> CalculateWeeklyProgressAsync();
        Task<UserProgress[]> CalculateDailyProgressAsync(string[] emails);
    }

    public record UserProgress(string Email, string Name, double Hours, bool BelowTarget);
}