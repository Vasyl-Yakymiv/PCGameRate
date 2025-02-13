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
    }
}
