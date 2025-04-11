using FootballGoal.API.Interfaces;
using FootballGoal.API.Models;
using System.Text.Json;

namespace FootballGoal.API.Services;

public class FootballGoalsService : IFootballGoalsService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FootballGoalsService> _logger;
    private const string API_URL = "https://jsonmock.hackerrank.com/api/football_matches";

    public FootballGoalsService(HttpClient httpClient, ILogger<FootballGoalsService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TeamGoalsResponse> CalculateTotalGoalsAsync(string teamName, int year)
    {
        _logger.LogInformation("Calculando gols para o time {Team} em {Year}", teamName, year);

        int goalsAsTeam1 = await GetGoalsByTeamRoleAsync(teamName, year, true);
        _logger.LogDebug("Gols como time1: {Goals}", goalsAsTeam1);

        int goalsAsTeam2 = await GetGoalsByTeamRoleAsync(teamName, year, false);
        _logger.LogDebug("Gols como time2: {Goals}", goalsAsTeam2);

        int totalGoals = goalsAsTeam1 + goalsAsTeam2;

        return new TeamGoalsResponse
        {
            TeamName = teamName,
            Year = year,
            TotalGoals = totalGoals
        };
    }

    public async Task<PredefinedTeamsGoalsResponse> CalculatePredefinedTeamsGoalsAsync()
    {
        _logger.LogInformation("Calculando gols para times predefinidos");

        var response = new PredefinedTeamsGoalsResponse();

        // Executar as consultas em paralelo para melhorar o desempenho
        var psgTask = CalculateTotalGoalsAsync("Paris Saint-Germain", 2013);
        var chelseaTask = CalculateTotalGoalsAsync("Chelsea", 2014);

        await Task.WhenAll(psgTask, chelseaTask);

        response.Teams.Add(await psgTask);
        response.Teams.Add(await chelseaTask);

        return response;
    }

    /// <summary>
    /// Obtém os gols marcados por um time, seja como time1 ou time2
    /// </summary>
    /// <param name="teamName">Nome do time</param>
    /// <param name="year">Ano</param>
    /// <param name="isTeam1">Se true, calcula como time1; se false, como time2</param>
    /// <returns>Total de gols</returns>
    private async Task<int> GetGoalsByTeamRoleAsync(string teamName, int year, bool isTeam1)
    {
        int totalGoals = 0;
        int currentPage = 1;
        int totalPages = 1;

        string teamParam = isTeam1 ? "team1" : "team2";
        string goalParam = isTeam1 ? "team1goals" : "team2goals";
        string role = isTeam1 ? "time1" : "time2";

        try
        {
            do
            {
                string url = $"{API_URL}?year={year}&{teamParam}={Uri.EscapeDataString(teamName)}&page={currentPage}";
                _logger.LogDebug("Consultando URL: {Url}", url);

                using var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var matchesResponse = JsonSerializer.Deserialize<FootballMatchesResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (matchesResponse is null)
                {
                    _logger.LogWarning("Resposta da API não pôde ser deserializada para {Team} como {Role}", teamName, role);
                    break;
                }

                if (currentPage == 1)
                {
                    totalPages = matchesResponse.Total_pages;
                    _logger.LogDebug("Total de páginas para {Team} como {Role}: {Pages}", teamName, role, totalPages);
                }

                foreach (var match in matchesResponse.Data)
                {
                    string goalsString = isTeam1 ? match.Team1goals : match.Team2goals;

                    if (!string.IsNullOrEmpty(goalsString) && int.TryParse(goalsString, out int goals))
                    {
                        totalGoals += goals;
                    }
                }

                currentPage++;
            } while (currentPage <= totalPages);

            _logger.LogInformation("Total de {Goals} gols para {Team} como {Role} em {Year}",
                totalGoals, teamName, role, year);

            return totalGoals;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erro HTTP ao buscar gols para {Team} como {Role}: {Message}",
                teamName, role, ex.Message);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Erro ao deserializar resposta para {Team} como {Role}: {Message}",
                teamName, role, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não esperado ao buscar gols para {Team} como {Role}: {Message}",
                teamName, role, ex.Message);
            throw;
        }
    }
}
