using System;
using System.Collections.Generic;

namespace ClockiSlackBot.Models
{
    // Game DTOs
    public record Story(int Id, string Title, string Intro, string DailyFail, string RiskHigh, string Win, string Loss);
    public record StoriesDocument(List<Story> Stories);
    public record VacationEntry(string Email, DateTime Date);
    public record VacationsDocument(List<VacationEntry> Vacations);
}