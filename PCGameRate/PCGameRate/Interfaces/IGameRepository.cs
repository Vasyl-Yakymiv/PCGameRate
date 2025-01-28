using PCGameRate.Models;
using System;

namespace PCGameRate.Interfaces
{
    public interface IGameRepository
    {
        Task<IEnumerable<Game>> GetAll();
        Task<Game?> GetByIdAsync(int id);
        Task<Game?> GetByIdAsyncNoTracking(int id);
        bool Add(Game game);
        bool Update(Game game);
        bool Save();
        bool Delete(Game game);

    }
}
