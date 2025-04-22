using PCGameRate.Models;
using System;

namespace PCGameRate.Interfaces
{
    public interface IAccountRepository
    {
        List<Game> GetRecommendedGames(List<Rating> userRatings);
        Task<List<Game?>> GetPopularGames();
        Task<List<Rating?>> GetUserRatings(string userId);
    }
}
