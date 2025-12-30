using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClockiSlackBot;

namespace ClockiSlackBot.Services
{
    public interface ISlackService
    {
        Task SendMessageAsync(string email, string text);
    }
}
