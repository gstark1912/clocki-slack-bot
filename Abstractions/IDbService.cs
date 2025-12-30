using System;
using ClockiSlackBot.Models;

namespace ClockiSlackBot.Abstractions
{
    public interface IDbService
    {
        Story GetStory(int storyId);
        bool IsVacation(DateTime date, string email);
        int GetStoriesCount();
    }
}