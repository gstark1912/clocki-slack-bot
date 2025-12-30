using System;
using ClockiSlackBot.Models;

namespace ClockiSlackBot.Services
{
    public interface IWeekService
    {
        DateTime GetWeekStart(DateTime date);
        bool ShouldStartNewWeek(GameState state);
        bool IsAlertDay(DateTime date);
        bool IsFinalDay(DateTime date);
    }
}