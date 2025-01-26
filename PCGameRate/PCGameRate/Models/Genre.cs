using System.ComponentModel.DataAnnotations;

namespace PCGameRate.Models
{
    public class Genre
    {
        [Key]
        public int GenreId { get; set; }
        public string GameGenre { get; set; }
    }
}
