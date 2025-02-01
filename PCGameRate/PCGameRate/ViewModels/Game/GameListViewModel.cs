using PCGameRate.Models;

namespace PCGameRate.ViewModels.Game
{
    public class GameListViewModel
    {
        public List<PCGameRate.Models.Game> Games { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
