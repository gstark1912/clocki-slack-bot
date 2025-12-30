using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ClockiSlackBot.Services
{
    public class DbService : IDbService
    {
        private readonly string _storiesPath;
        private readonly string _vacationsPath;
        private StoriesDocument? _stories;
        private VacationsDocument? _vacations;

        public DbService()
        {
            var dataDir = "data";
            _storiesPath = Path.Combine(dataDir, "stories.json");
            _vacationsPath = Path.Combine(dataDir, "vacations.json");
        }

        private StoriesDocument LoadStories()
        {
            if (_stories == null)
            {
                var json = File.ReadAllText(_storiesPath);
                _stories = JsonSerializer.Deserialize<StoriesDocument>(json)!;
            }
            return _stories;
        }

        private VacationsDocument? LoadVacations()
        {
            if (_vacations == null && File.Exists(_vacationsPath))
            {
                var json = File.ReadAllText(_vacationsPath);
                _vacations = JsonSerializer.Deserialize<VacationsDocument>(json)!;
            }
            return _vacations;
        }

        public Story GetStory(int storyId)
        {
            var stories = LoadStories();
            return stories.Stories.Find(s => s.Id == storyId) ?? stories.Stories[0];
        }

        public bool IsVacation(DateTime date, string email)
        {
            var vacations = LoadVacations();
            if (vacations == null) return false;
            
            return vacations.Vacations.Any(entry => 
                entry.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && 
                entry.Date.Date == date.Date);
        }

        public int GetStoriesCount()
        {
            var stories = LoadStories();
            return stories.Stories.Count;
        }
    }
}