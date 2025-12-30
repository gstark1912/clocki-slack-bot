using System;
using ClockiSlackBot.Models;

namespace ClockiSlackBot.Services
{
    public class WeekService : IWeekService
    {
        public DateTime GetWeekStart(DateTime date)
        {
            var dayOfWeek = date.DayOfWeek;
            return date.AddDays(-(int)dayOfWeek + (dayOfWeek == DayOfWeek.Sunday ? -6 : 1));
        }

        public bool ShouldStartNewWeek(GameState state)
        {
            var weekStart = GetWeekStart(DateTime.UtcNow.Date);
            return state.Status == GameStatus.NotStarted && state.LastUpdate.Date < weekStart;
        }

        public bool IsAlertDay(DateTime date) => date.DayOfWeek == DayOfWeek.Thursday;

        public bool IsFinalDay(DateTime date) => date.DayOfWeek == DayOfWeek.Friday;
    }
}