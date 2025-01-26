using Microsoft.EntityFrameworkCore;
using PCGameRate.Data;
using PCGameRate.Interfaces;
using PCGameRate.Models;

namespace PCGameRate.Repository
{
    public class GameRepository : IGameRepository
    {
        ApplicationDbContext _context;
        public GameRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Game>> GetAll()
        {
            return await _context.Games.Include(i => i.Developer).Include(x => x.Genre).ToListAsync();
        }

        public async Task<Game> GetByIdAsync(int id)
        {
            return await _context.Games.Include(i => i.Developer).Include(x => x.Genre).FirstOrDefaultAsync(i => i.GameId == id);
        }
    }
}
