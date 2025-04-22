using PCGameRate.Models;
using System;

namespace PCGameRate.Interfaces
{
    public interface IGameRepository
    {
        Task<IEnumerable<Game>> GetAll();
        Task<IEnumerable<Game>> GetTop100();
        Task<Game?> GetByIdAsync(int id);
        Task<Game?> GetWithReviewAndScreenshotsByIdAsync(int id);
        Task<Game?> GetByIdAsyncNoTracking(int id);
        Task<int?> GetUserRating(int id, string userId);
        Task<List<Game?>> SearchResult(string query);
        Task<List<Game?>> GetExpectedGames();
        Task<List<Game?>> GetTopSortedGames();
        bool Add(Game game);
        bool Update(Game game);
        bool Save();
        bool Delete(Game game);

    }
}
