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
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        [ForeignKey("Game")]
        public int GameId { get; set; }
        public Game Game { get; set; }
        [ForeignKey("User")]
        public string? Id { get; set; }
        public User? User { get; set; }
        public List<Vote> Votes { get; set; } = new List<Vote>();
    }
}
