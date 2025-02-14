using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PCGameRate.Models
{
    public class GameVideo
    {
        [Key]
        public int VideoId { get; set; }
        [ForeignKey("Game")]
        public int GameId { get; set; }
        public Game Game { get; set; }
        [Required]
        [Url]
        public string VideoUrl { get; set; }
        public string Type { get; set; } = "trailer";
    }
}
