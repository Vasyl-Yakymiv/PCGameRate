using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PCGameRate.Models;

namespace PCGameRate.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }
        public DbSet<Game> Games { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Developer> Developers { get; set; }
        public DbSet<Screenshot> Screenshots { get; set; }
        public DbSet<GameVideo> GameVideos { get; set; }
        public DbSet<ReviewVote> ReviewVotes { get; set; }
        public DbSet<Vote> Votes { get; set; }
    }
}
