using System.Threading.Tasks;

namespace ClockiSlackBot.Abstractions
{
    public interface ISlackService
    {
        Task SendMessageAsync(string email, string text);
    }
}