using System;

namespace ClockiSlackBot.Logger
{
    public class LoggerService : ILoggerService
    {
        public void Log(string message)
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
        }
    }
}