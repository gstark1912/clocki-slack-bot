using System;
using System.Collections.Generic;

namespace ClockiSlackBot.Services
{
    public interface IDbService
    {
        Story GetStory(int storyId);
        bool IsVacation(DateTime date, string email);
        int GetStoriesCount();
    }
}