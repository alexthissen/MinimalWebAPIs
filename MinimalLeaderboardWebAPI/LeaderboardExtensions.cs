using Microsoft.EntityFrameworkCore;
using MinimalLeaderboardWebAPI.Infrastructure;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("MinimalLeaderboardWebAPI.Tests")]

namespace MinimalLeaderboardWebAPI
{
    public static class LeaderboardExtensions
    {
        public static void MapLeaderboard(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/leaderboard", GetScores)
                .WithDescription("Gets all highscores")
                .WithName("GetScores")
                .Produces<IEnumerable<HighScore>>(StatusCodes.Status200OK)
                .Produces<IEnumerable<HighScore>>(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .AllowAnonymous();
        }

        internal static async Task<IResult> GetScores(LeaderboardContext context, ILogger<Program> logger, int limit = 10)
        {
            logger?.LogInformation("Retrieving score list with a limit of {SearchLimit}.", limit);

            var scores = context.Scores
                .Select(score => new HighScore()
                {
                    Game = score.Game,
                    Points = score.Points,
                    Nickname = score.Gamer.Nickname
                }).Take(limit);

            return TypedResults.Ok(await scores.ToListAsync().ConfigureAwait(false));
        }
    }
}
