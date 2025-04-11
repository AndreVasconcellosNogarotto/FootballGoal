namespace FootballGoal.API.Models;

public class TeamGoalsResponse
{
    public string TeamName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int TotalGoals { get; set; }
    public string Message => $"Team {TeamName} scored {TotalGoals} goals in {Year}";
}
