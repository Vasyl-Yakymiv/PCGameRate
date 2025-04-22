using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using PCGameRate.Data;
using PCGameRate.Interfaces;
using PCGameRate.Models;

namespace PCGameRate.Repository
{
    public class AccountRepository : IAccountRepository
    {
        ApplicationDbContext _context;

        public AccountRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Game?>> GetPopularGames()
        {
            return await _context.Games
                        .Where(g => g.IsPopular)
                        .Include(i => i.Developer)
                        .Include(x => x.Genre)
                        .Take(10)
                        .ToListAsync();
        }

        public List<Game> GetRecommendedGames(List<Rating> userRatings)
        {
            
            var allGames = _context.Games.Include(g => g.Genre).Include(g => g.Developer).ToList(); 

            var ratedGenres = userRatings.Select(r => r.Game.Genre.GameGenre).Distinct().ToList();

           
            var recommendedGames = allGames
                                    .Where(g => ratedGenres.Contains(g.Genre.GameGenre)
                                                && !userRatings.Any(r => r.Game.GameId == g.GameId)) 
                                    .ToList();

            
            var randomRecommendedGames = recommendedGames
                                         .OrderBy(g => Guid.NewGuid())
                                         .Take(10)                    
                                         .ToList();

            return randomRecommendedGames;
        }

        public async Task<List<Rating?>> GetUserRatings(string userId)
        {
           return await _context.Ratings
                        .Where(r => r.User.UserName == userId)
                        .ToListAsync();
        }
    }
}
