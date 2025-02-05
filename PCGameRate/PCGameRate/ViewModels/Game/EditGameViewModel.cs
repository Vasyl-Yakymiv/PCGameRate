using PCGameRate.Models;

namespace PCGameRate.ViewModels.Game
{
    public class EditGameViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string ReleaseDate { get; set; }
        public float? RatingAverage { get; set; }
        public int RatingCount { get; set; }
        public Developer Developer { get; set; }
        public Genre Genre { get; set; }
    }
}
