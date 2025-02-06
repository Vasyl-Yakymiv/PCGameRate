using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PCGameRate.Models
{
    public class Game
    {
        [Key]
        public int GameId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string ReleaseDate { get; set; }
        public float? RatingAverage { get; set; }
        public bool? IsPopular { get; set; }
        public bool? IsExpected { get; set; }
        public int RatingCount { get; set; }
        [ForeignKey("Developer")]
        public int DeveloperId { get; set; }
        public Developer Developer { get; set; }
        [ForeignKey("Genre")]
        public int GenreId { get; set; }
        public Genre Genre { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Screenshot> Screenshots { get; set; }
    }
}
