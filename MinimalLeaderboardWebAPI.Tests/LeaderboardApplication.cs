using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MinimalLeaderboardWebAPI.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace MinimalLeaderboardWebAPI.Tests
{
    internal class LeaderboardApplication: WebApplicationFactory<Program>
    {
        private readonly string environment;

        public LeaderboardApplication(string environment = "Development")
        {
            this.environment = environment;
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var root = new InMemoryDatabaseRoot();

            builder.ConfigureServices(services => {
                services.AddScoped(provider =>
                {
                    // Replace SQL Server with in-memory provider
                    return new DbContextOptionsBuilder<LeaderboardContext>()
                    .UseInMemoryDatabase("HighScores", root)
                    .UseApplicationServiceProvider(provider)
                    .Options;
                });
            });
            return base.CreateHost(builder);
        }
    }
}
