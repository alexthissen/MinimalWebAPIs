﻿using LeaderboardWebAPI.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaderboardWebAPI.Controllers
{
    public class HighScore
    {
        public string Game { get; set; }
        public string Nickname { get; set; }
        public int Points { get; set; }
    }

    [ApiController]
    [Produces("application/xml", "application/json")]
    [Route("api/[controller]")]
    public class LeaderboardController : ControllerBase
    {
        public LeaderboardContext context { get; }

        private readonly ILogger<LeaderboardController> logger;

        public LeaderboardController(LeaderboardContext context, ILoggerFactory loggerFactory)
        {
            this.context = context;
            this.logger = loggerFactory.CreateLogger<LeaderboardController>();
        }

        // GET api/leaderboard
        /// <summary>
        /// Retrieve a list of leaderboard scores.
        /// </summary>
        /// <returns>List of high scores per game.</returns>
        /// <response code="200">The list was successfully retrieved.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<HighScore>), 200)]
        public async Task<ActionResult<IEnumerable<HighScore>>> Get(int limit = 10)
        {
            logger?.LogInformation("Retrieving score list with a limit of {SearchLimit}.", limit);

            var scores = context.Scores
                .Select(score => new HighScore()
                {
                    Game = score.Game,
                    Points = score.Points,
                    Nickname = score.Gamer.Nickname
                }).Take(limit);

            return Ok(await scores.ToListAsync().ConfigureAwait(false));
        }
    }
}
