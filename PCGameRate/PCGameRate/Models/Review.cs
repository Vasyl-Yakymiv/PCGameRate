using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PCGameRate.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }
        public string ReviewText { get; set; }
        public DateTime? DatePosted { get; set; } = DateTime.Now;
        [ForeignKey("Game")]
        public int GameId { get; set; }
        public Game Game { get; set; }
        [ForeignKey("User")]
        public string? Id { get; set; }
        public User? User { get; set; }
    }
}
