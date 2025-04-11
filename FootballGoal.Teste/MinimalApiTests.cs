using FootballGoal.API.Interfaces;
using FootballGoal.API.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Net.Http.Json;
namespace FootballGoal.Teste;
public class MinimalApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public MinimalApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task HealthCheck_DeveRetornarOk()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", content);
    }
}

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remover o serviço real
            var serviceDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IFootballGoalsService));

            if (serviceDescriptor != null)
            {
                services.Remove(serviceDescriptor);
            }

            // Adicionar o serviço mockado
            var mockService = new Mock<IFootballGoalsService>();

            mockService.Setup(s => s.CalculateTotalGoalsAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new TeamGoalsResponse
                {
                    TeamName = "Test Team",
                    Year = 2023,
                    TotalGoals = 42
                });

            mockService.Setup(s => s.CalculatePredefinedTeamsGoalsAsync())
                .ReturnsAsync(new PredefinedTeamsGoalsResponse
                {
                    Teams = new List<TeamGoalsResponse>
                    {
                            new() { TeamName = "PSG", Year = 2013, TotalGoals = 109 },
                            new() { TeamName = "Chelsea", Year = 2014, TotalGoals = 92 }
                    }
                });

            services.AddScoped<IFootballGoalsService>(sp => mockService.Object);
        });
    }
}