using PCGameRate.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace PCGameRate.ViewModels.Game
{
    public class IndexGameViewModel
    {
        public string Title { get; set; }
        public string Image { get; set; }
        public string ReleaseDate { get; set; }
        public float RatingAverage { get; set; }
        public Developer Developer { get; set; }
        public Genre Genre { get; set; }
    }
}
