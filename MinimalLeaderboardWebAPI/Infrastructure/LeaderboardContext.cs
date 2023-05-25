using MinimalLeaderboardWebAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace MinimalLeaderboardWebAPI.Infrastructure
{
    public class LeaderboardContext : IdentityDbContext<IdentityUser>
    {
        public LeaderboardContext(DbContextOptions<LeaderboardContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Use entity configuration
            modelBuilder.ApplyConfiguration(new GamerConfiguration());

            // Or configure entity here
            modelBuilder.Entity<Score>()
                .ToTable("Scores")
                .HasData(
                    new Score() { Id = 1, GamerId = 1, Points = 1337, Game = "Pac-man" },
                    new Score() { Id = 2, GamerId = 2, Points = 42, Game = "Donkey Kong" }
                );

            // Call base class to create other models in derived DbContext
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Gamer> Gamers { get; set; }
        public DbSet<Score> Scores { get; set; }
    }
}