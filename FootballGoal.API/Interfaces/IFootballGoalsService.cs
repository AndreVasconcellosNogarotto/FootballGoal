using FootballGoal.API.Models;

namespace FootballGoal.API.Interfaces;

public interface IFootballGoalsService
{
    Task<TeamGoalsResponse> CalculateTotalGoalsAsync(string teamName, int year);
    Task<PredefinedTeamsGoalsResponse> CalculatePredefinedTeamsGoalsAsync();
}
