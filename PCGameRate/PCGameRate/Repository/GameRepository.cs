using Microsoft.EntityFrameworkCore;
using PCGameRate.Data;
using PCGameRate.Interfaces;
using PCGameRate.Models;
using System;

namespace PCGameRate.Repository
{
    public class GameRepository : IGameRepository
    {
        ApplicationDbContext _context;
        public GameRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool Add(Game game)
        {
            _context.Add(game);
            return Save();
        }

        public bool Delete(Game game)
        {
            _context.Remove(game);
            return Save();
        }

        public async Task<IEnumerable<Game>> GetAll()
        {
            return await _context.Games.Include(i => i.Developer).Include(x => x.Genre).ToListAsync();
        }

       

        public async Task<Game> GetByIdAsync(int id)
        {
            return await _context.Games.Include(i => i.Developer).Include(x => x.Genre).FirstOrDefaultAsync(i => i.GameId == id);
        }

        public async Task<Game?> GetByIdAsyncNoTracking(int id)
        {
            return await _context.Games
                .Include(i => i.Developer)
                .Include(x => x.Genre)
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.GameId == id);
        }

        public async Task<List<Game?>> GetExpectedGames()
        {
            return await _context.Games
                .Where(g => (bool)g.IsExpected)
                .ToListAsync();
        }

        public async Task<IEnumerable<Game>> GetTop100()
        {
            return await _context.Games
                .Where(g => g.RatingCount >= 1000)
                .OrderByDescending(g => g.RatingAverage)
                .ThenByDescending(g => g.RatingCount)
                .Take(100)
                .ToListAsync();
        }

        public async Task<List<Game?>> GetTopSortedGames()
        {
            return await _context.Games
                .Where(g => g.RatingCount >= 1000)
                 .ToListAsync();
        }

        public async Task<int?> GetUserRating(int id, string userId)
        {
            return await _context.Ratings
                .Where(r => r.GameId == id && r.Id == userId)
                .Select(r => r.RatingValue)
                .FirstOrDefaultAsync();
        }

        public async Task<Game?> GetWithReviewAndScreenshotsByIdAsync(int id)
        {
            return await _context.Games.Include(v => v.GameVideos).Include(s => s.Screenshots).Include(g => g.Reviews).ThenInclude(r => r.User).FirstOrDefaultAsync(g => g.GameId == id);
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0;
        }

        public async Task<List<Game?>> SearchResult(string query)
        {
            return await _context.Games
                .Where(g => g.Title.Contains(query))
                .ToListAsync();
        }

        public bool Update(Game game)
        {
            _context.Update(game);
            return Save();
        }

       
    }
}
