using System;
using System.Linq;
using System.Threading.Tasks;
using ClockiSlackBot.Config;
using ClockiSlackBot.Abstractions;

namespace ClockiSlackBot.Services
{
    public class ProgressCalculator : IProgressCalculator
    {
        private readonly IClockifyService _clockifyService;
        private readonly IWeekService _weekService;
        private readonly IDbService _dbService;
        private readonly IGameConfig _config;

        public ProgressCalculator(IClockifyService clockifyService, IWeekService weekService, IDbService dbService, IGameConfig config)
        {
            _clockifyService = clockifyService;
            _weekService = weekService;
            _dbService = dbService;
            _config = config;
        }

        public async Task<double> CalculateWeeklyProgressAsync()
        {
            var weekStart = _weekService.GetWeekStart(DateTime.UtcNow);
            var report = await _clockifyService.GetDailySummaryAsync(weekStart);
            var users = await _clockifyService.GetUsersAsync();
            double totalSeconds = report?.GroupOne?.Sum(g => g.Duration) ?? 0;
            double targetSeconds = _config.TargetHours * 3600 * users.Count * 7;
            return targetSeconds == 0 ? 0 : (totalSeconds / targetSeconds) * 100.0;
        }

        public async Task<UserProgress[]> CalculateDailyProgressAsync(string[] emails)
        {
            var users = await _clockifyService.GetUsersAsync();
            var report = await _clockifyService.GetDailySummaryAsync(DateTime.UtcNow);
            
            return emails
                .Where(email => !_dbService.IsVacation(DateTime.UtcNow, email))
                .Select(email =>
                {
                    var user = users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
                    if (user == null) return null;
                    
                    var entry = report?.GroupOne?.FirstOrDefault(g => g.Name.Equals(user.Name, StringComparison.OrdinalIgnoreCase));
                    double hours = entry?.Duration / 3600.0 ?? 0.0;
                    
                    return new UserProgress(email, user.Name, hours, hours < _config.TargetHours);
                })
                .Where(up => up != null)
                .ToArray()!;
        }
    }
}