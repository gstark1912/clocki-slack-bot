using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ClockiSlackBot.Models;
using ClockiSlackBot.Abstractions;

namespace ClockiSlackBot.Repositories
{
    public class GameStateRepository : IGameStateRepository
    {
        private readonly string _gameStatePath;

        public GameStateRepository(string dataDir = "data")
        {
            _gameStatePath = Path.Combine(dataDir, "gamestate.json");
        }

        public async Task<GameState> LoadAsync()
        {
            if (!File.Exists(_gameStatePath))
            {
                var init = new GameState(0, DateTime.UtcNow, 1, GameStatus.NotStarted);
                await SaveAsync(init);
                return init;
            }
            var json = await File.ReadAllTextAsync(_gameStatePath);
            return JsonSerializer.Deserialize<GameState>(json)!;
        }

        public async Task SaveAsync(GameState state)
        {
            var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_gameStatePath, json);
        }
    }
}