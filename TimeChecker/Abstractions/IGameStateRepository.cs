using System.Threading.Tasks;
using ClockiSlackBot.Models;

namespace ClockiSlackBot.Abstractions
{
    public interface IGameStateRepository
    {
        Task<GameState> LoadAsync();
        Task SaveAsync(GameState state);
    }
}