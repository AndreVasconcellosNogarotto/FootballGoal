using FootballGoal.API.Interfaces;
using FootballGoal.API.Models;
using FootballGoal.API.Services;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;


public partial class Program
{
    public static void Main(string[] args)
    {
        var app = CreateWebApplication(args);
        app.Run();
    }

    // Método para criar a aplicação web - útil para testes
    public static WebApplication CreateWebApplication(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Adicionar serviços
        ConfigureServices(builder);

        var app = builder.Build();

        // Configurar o pipeline HTTP
        ConfigureApp(app);

        return app;
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Football Goals API",
                Version = "v1",
                Description = "API para cálculo de gols de times de futebol"
            });
        });

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        });

        builder.Services.AddHttpClient<IFootballGoalsService, FootballGoalsService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        builder.Services.AddScoped<IFootballGoalsService, FootballGoalsService>();
    }

    private static void ConfigureApp(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Football Goals API v1"));
        }

        app.UseHttpsRedirection();

        // Endpoint para gols de um time
        app.MapGet("/api/football/goals", async (
            string teamName,
            int year,
            IFootballGoalsService footballService,
            ILogger<Program> logger) =>
        {
            logger.LogInformation("Solicitando gols para o time {Team} no ano {Year}", teamName, year);
            try
            {
                var result = await footballService.CalculateTotalGoalsAsync(teamName, year);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao calcular gols para {Team} em {Year}", teamName, year);
                return Results.Problem(
                    title: "Erro ao processar a solicitação",
                    detail: ex.Message,
                    statusCode: 500
                );
            }
        })
        .WithName("GetTeamGoals")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Calcula a quantidade de gols marcados por um time em um ano específico";
            operation.Description = "Retorna os gols marcados por um time tanto como time da casa quanto como visitante";
            operation.Parameters[0].Description = "Nome do time";
            operation.Parameters[1].Description = "Ano para calcular os gols";
            return operation;
        })
        .Produces<TeamGoalsResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError);

        // Endpoint para times predefinidos
        app.MapGet("/api/football/predefined-goals", async (
            IFootballGoalsService footballService,
            ILogger<Program> logger) =>
        {
            logger.LogInformation("Solicitando gols para times predefinidos");
            try
            {
                var result = await footballService.CalculatePredefinedTeamsGoalsAsync();
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao calcular gols para times predefinidos");
                return Results.Problem(
                    title: "Erro ao processar a solicitação",
                    detail: ex.Message,
                    statusCode: 500
                );
            }
        })
        .WithName("GetPredefinedTeamsGoals")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Calcula a quantidade de gols dos times predefinidos";
            operation.Description = "Retorna os gols marcados por Paris Saint-Germain em 2013 e Chelsea em 2014";
            return operation;
        })
        .Produces<PredefinedTeamsGoalsResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError);

        // Endpoint de health check
        app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }))
            .WithName("HealthCheck")
            .WithOpenApi();

        app.UseStatusCodePages();
    }
}