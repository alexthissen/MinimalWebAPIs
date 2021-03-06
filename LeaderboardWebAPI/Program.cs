using LeaderboardWebAPI.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace LeaderboardWebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args)
                .Build()
                .Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((context, builder) =>
                {
                    builder.AddApplicationInsights(options =>
                    {
                        options.IncludeScopes = true;
                        options.TrackExceptionsAsExceptionTelemetry = true;
                    });
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    Assembly startupAssembly = typeof(Startup).GetTypeInfo().Assembly;
                    webBuilder.UseStartup(startupAssembly.GetName().Name);
                });
    }
}
