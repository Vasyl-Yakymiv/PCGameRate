using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PCGameRate.Models
{
    public class Rating
    {
        [Key]
        public int RatingId { get; set; }
        [ForeignKey("Game")]
        public int? GameId { get; set; }
        public Game? Game {  get; set; }
        [ForeignKey("User")]
        public string? Id { get; set; }
        public User? User { get; set; }
        public int? RatingValue { get; set; }
        public DateTime? RatingDate { get; set; } = DateTime.Now;
    }
}
