using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using MinimalLeaderboardWebAPI.Infrastructure;
using MinimalLeaderboardWebAPI.Models;
using System.Net.Http.Json;

namespace MinimalLeaderboardWebAPI.Tests
{
    [TestClass]
    public class LeaderboardWebApiTest
    {
        [TestMethod]
        public async Task GetReturnsList()
        {
            await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var root = new InMemoryDatabaseRoot();
                    services.AddScoped(provider =>
                    {
                        // Replace SQL Server with in-memory provider
                        return new DbContextOptionsBuilder<LeaderboardContext>()
                            .UseInMemoryDatabase("HighScores", root)
                            .UseApplicationServiceProvider(provider)
                            .Options;
                    });
                });
            });

            // Create direct in-memory HTTP client
            HttpClient client = application.CreateClient(new WebApplicationFactoryClientOptions() { });
            var response = await client.GetFromJsonAsync<IEnumerable<HighScore>>("/api/leaderboard");

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(2, response.Count());
        }

        [TestMethod]
        public async Task GetReturnsOkResult()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<LeaderboardContext>()
                .UseInMemoryDatabase(databaseName: "HighScores")
                .Options;
            using var context = new LeaderboardContext(options);
            
            context.Gamers.Add(new Gamer() {
                Id = 1, GamerGuid = Guid.NewGuid(), Nickname = "LX360",
                Scores = new Score[] { new Score() { Id = 1, GamerId = 1, Points = 1234, Game = "Pac-man" } }
            });
            context.SaveChanges();
            
            // Act
            var result = await LeaderboardExtensions.GetScores(context, null, 10);

            // Assert
            Assert.IsInstanceOfType(result, typeof(Ok<List<HighScore>>));
            var httpResult = (Ok<List<HighScore>>)result;
            Assert.AreEqual(StatusCodes.Status200OK, httpResult.StatusCode);
            Assert.IsNotNull(httpResult.Value);
        }
    }
}