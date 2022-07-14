// docker run -e ACCEPT_EULA=Y -e MSSQL_PID=Developer -e SA_PASSWORD="Pass@word" --name sqldocker -p 5433:1433 -d mcr.microsoft.com/mssql/server:2022-latest

using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MinimalLeaderboardWebAPI;
using MinimalLeaderboardWebAPI.Infrastructure;
using MinimalLeaderboardWebAPI.Models;
using System.Text.Json.Serialization;

WebApplicationBuilder builder = WebApplication.CreateBuilder(
    new WebApplicationOptions()
    {
        Args = args,
        EnvironmentName = Environments.Development,
        ContentRootPath = Directory.GetCurrentDirectory(),
        WebRootPath = "webroot" 
    });

builder.Services.ConfigureRouteHandlerJsonOptions(options =>
{
    //Ignore Cycles
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "LeaderboardWebAPI", Version = "v1" });
});

builder.Services.AddHealthChecks();

// Add application insights
builder.Services.AddSingleton<ITelemetryInitializer, ServiceNameInitializer>();
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration);
builder.Logging.AddApplicationInsights(options =>
{
    options.IncludeScopes = true;
    options.TrackExceptionsAsExceptionTelemetry = true;
});

// Entity Framework
builder.Services.AddDbContext<LeaderboardContext>(options =>
{
    string connectionString = builder.Configuration.GetConnectionString("LeaderboardContext");
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
        maxRetryCount: 5,
        maxRetryDelay: TimeSpan.FromSeconds(30),
        errorNumbersToAdd: null);
    });
    //options.UseInMemoryDatabase(databaseName: "HighScores");
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // ApplicationServices does not exist anymore
    using (var scope = app.Services.CreateScope())
    {
        scope.ServiceProvider.GetRequiredService<LeaderboardContext>().Database.EnsureCreated();
    }
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "LeaderboardWebAPI v1"));
}

app.UseHttpsRedirection();

app.MapPost("/api/scores/{nickname}/{game}",
    [EndpointSummary("")] async Task<Results<Ok, NoContent, BadRequest>> (string nickname, string game, [FromBody] int points, LeaderboardContext context) =>
{
    // Lookup gamer based on nickname
    Gamer gamer = await context.Gamers
          .FirstOrDefaultAsync(g => g.Nickname.ToLower() == nickname.ToLower())
          .ConfigureAwait(false);

    if (gamer == null) return TypedResults.BadRequest();

    // Find highest score for game
    var score = await context.Scores
          .Where(s => s.Game == game && s.Gamer == gamer)
          .OrderByDescending(s => s.Points)
          .FirstOrDefaultAsync()
          .ConfigureAwait(false);

    if (score == null)
    {
        score = new Score() { Gamer = gamer, Points = points, Game = game };
        await context.Scores.AddAsync(score);
    }
    else
    {
        if (score.Points > points) return TypedResults.NoContent();
        score.Points = points;
    }
    await context.SaveChangesAsync().ConfigureAwait(false);
    return TypedResults.Ok();
})
    //.WithOpenApi(operation => {
    //    operation.Summary = "Upload potential new high scores";
    //    operation.Parameters[0].AllowEmptyValue = false;
    //    operation.OperationId = "PostHighScoreId";
    //    return operation;
    //})
    .WithDescription("Upload potential new high scores")
    .WithName("PostHighScore")
    .Accepts<int>("application/json", "application/xml"); // Limited support for XML

app.MapGet("/api/leaderboard", GetScores)
    .WithDescription("Gets all highscores")
    .WithName("GetScores")
    .Produces<IEnumerable<HighScore>>(StatusCodes.Status200OK)
    .Produces<IEnumerable<HighScore>>(StatusCodes.Status204NoContent)
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .AllowAnonymous();

async Task<IResult> GetScores(LeaderboardContext context, ILogger<Program> logger, int limit = 10)
{
    logger?.LogInformation("Retrieving score list with a limit of {SearchLimit}.", limit);

    var scores = context.Scores
        .Select(score => new HighScore()
        {
            Game = score.Game,
            Points = score.Points,
            Nickname = score.Gamer.Nickname
        }).Take(limit);
    
    return Results.Ok(await scores.ToListAsync().ConfigureAwait(false));
}

app.MapPost("/upload", async (IFormFile file) =>
{
    // Make sure to security check uploaded file
    using var stream = File.OpenWrite("upload.txt");
    await file.CopyToAsync(stream);
})
    .ExcludeFromDescription();

app.Logger.LogInformation("Logging before run");
app.Run();

public record struct HighScore
{
    public string Game { get; set; }
    public string Nickname { get; set; }
    public int Points { get; set; }
}

// Add line below to make Program class public for testing
// Or use [assembly:InternalsVisibleTo("")]
//public partial class Program { } 
