using FootballGoal.API.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;

namespace FootballGoal.Teste;
public class FootballGoalsServiceTests
{
    private readonly Mock<ILogger<FootballGoalsService>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly FootballGoalsService _service;


    public FootballGoalsServiceTests()
    {
        _loggerMock = new Mock<ILogger<FootballGoalsService>>();
        _httpHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpHandlerMock.Object);
        _service = new FootballGoalsService(_httpClient, _loggerMock.Object);
    }

    [Fact]
    public async Task CalculateTotalGoalsAsync_DeveCalcularCorretamente()
    {
        // Arrange - Configure o mock para retornar respostas para time1 e time2
        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.Query.Contains("team1=TestTeam")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"{
                        ""page"": 1,
                        ""per_page"": 10,
                        ""total"": 5,
                        ""total_pages"": 1,
                        ""data"": [
                            {
                                ""team1"": ""TestTeam"",
                                ""team2"": ""OtherTeam"",
                                ""team1goals"": ""3"",
                                ""team2goals"": ""1""
                            }
                        ]
                    }")
            });

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.Query.Contains("team2=TestTeam")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"{
                        ""page"": 1,
                        ""per_page"": 10,
                        ""total"": 5,
                        ""total_pages"": 1,
                        ""data"": [
                            {
                                ""team1"": ""OtherTeam"",
                                ""team2"": ""TestTeam"",
                                ""team1goals"": ""1"",
                                ""team2goals"": ""2""
                            }
                        ]
                    }")
            });

        // Act
        var result = await _service.CalculateTotalGoalsAsync("TestTeam", 2023);

        // Assert
        Assert.Equal("TestTeam", result.TeamName);
        Assert.Equal(2023, result.Year);
        Assert.Equal(5, result.TotalGoals); // 3 como time1 + 2 como time2
    }
}
