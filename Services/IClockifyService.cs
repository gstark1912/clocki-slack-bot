using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClockiSlackBot.Models;

namespace ClockiSlackBot.Services
{
    public interface IClockifyService
    {
        Task<ClockifySummaryResponse?> GetDailySummaryAsync(DateTime date);
    Task<List<ClockifyUser>> GetUsersAsync();
    }
}
