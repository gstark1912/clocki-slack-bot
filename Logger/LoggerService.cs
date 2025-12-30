using System;

namespace ClockiSlackBot.Logger
{
    public class LoggerService
    {
        public void Log(string message)
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
        }
    }
}