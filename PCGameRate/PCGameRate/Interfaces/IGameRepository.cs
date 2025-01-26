using PCGameRate.Models;

namespace PCGameRate.Interfaces
{
    public interface IGameRepository
    {
        Task<IEnumerable<Game>> GetAll();
        Task<Game?> GetByIdAsync(int id);
    }
}
