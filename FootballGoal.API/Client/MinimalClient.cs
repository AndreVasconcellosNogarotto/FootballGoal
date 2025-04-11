using FootballGoal.API.Models;
using System.Text.Json;

namespace FootballGoal.Client;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Inicializando cliente para cálculo de gols...");

        try
        {
            var apiBaseUrl = "https://localhost:5001";
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(2);

            await MostrarMenu(client);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ocorreu um erro: {ex.Message}");

            if (ex is HttpRequestException httpEx)
            {
                Console.WriteLine($"Erro HTTP: {httpEx.StatusCode}");
            }
            else if (ex is TaskCanceledException)
            {
                Console.WriteLine("A operação excedeu o tempo limite.");
            }
            else if (ex is JsonException jsonEx)
            {
                Console.WriteLine($"Erro ao processar JSON: {jsonEx.Message}");
            }
        }

        Console.WriteLine("\nPressione qualquer tecla para sair...");
        Console.ReadKey();
    }

    private static async Task MostrarMenu(HttpClient client)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("===== Cliente da Football Goals API =====");
            Console.WriteLine("1. Consultar gols de um time específico");
            Console.WriteLine("2. Consultar times predefinidos");
            Console.WriteLine("3. Verificar status da API");
            Console.WriteLine("0. Sair");
            Console.Write("\nEscolha uma opção: ");

            if (!int.TryParse(Console.ReadLine(), out int opcao))
            {
                Console.WriteLine("Opção inválida. Pressione qualquer tecla para continuar...");
                Console.ReadKey();
                continue;
            }

            switch (opcao)
            {
                case 0:
                    return;
                case 1:
                    await ConsultarTimeEspecifico(client);
                    break;
                case 2:
                    await ConsultarTimesPredefinidos(client);
                    break;
                case 3:
                    await VerificarStatusAPI(client);
                    break;
                default:
                    Console.WriteLine("Opção inválida. Pressione qualquer tecla para continuar...");
                    break;
            }

            Console.WriteLine("\nPressione qualquer tecla para continuar...");
            Console.ReadKey();
        }
    }

    private static async Task ConsultarTimeEspecifico(HttpClient client)
    {
        Console.Write("\nInforme o nome do time: ");
        string teamName = Console.ReadLine() ?? "Paris Saint-Germain";

        Console.Write("Informe o ano: ");
        if (!int.TryParse(Console.ReadLine(), out int year))
        {
            year = 2013;
            Console.WriteLine($"Ano inválido, usando o padrão: {year}");
        }

        Console.WriteLine($"\nConsultando gols para {teamName} em {year}...");

        var response = await client.GetFromJsonAsync<TeamGoalsResponse>(
            $"/api/football/goals?teamName={Uri.EscapeDataString(teamName)}&year={year}");

        if (response != null)
        {
            Console.WriteLine("\nResultado:");
            Console.WriteLine(response.Message);
        }
        else
        {
            Console.WriteLine("Nenhum resultado encontrado.");
        }
    }

    private static async Task ConsultarTimesPredefinidos(HttpClient client)
    {
        Console.WriteLine("\nConsultando times predefinidos...");

        var response = await client.GetFromJsonAsync<PredefinedTeamsGoalsResponse>("/api/football/predefined-goals");

        if (response != null && response.Teams.Count > 0)
        {
            Console.WriteLine("\nResultados:");
            foreach (var team in response.Teams)
            {
                Console.WriteLine(team.Message);
            }
        }
        else
        {
            Console.WriteLine("Nenhum resultado encontrado.");
        }
    }

    private static async Task VerificarStatusAPI(HttpClient client)
    {
        Console.WriteLine("\nVerificando status da API...");

        var response = await client.GetAsync("/health");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"API está ativa: {content}");
        }
        else
        {
            Console.WriteLine($"API não está respondendo. Status: {response.StatusCode}");
        }
    }
}