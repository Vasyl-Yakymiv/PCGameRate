using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PCGameRate.Models
{
    public class Screenshot
    {
        [Key]
        public int PhotoId { get; set; }
        [ForeignKey("Game")]
        public int? GameId { get; set; }
        public Game? Game { get; set; }
        [Required]
        [Url]
        public string ScreenshotUrl { get; set; }
    }
}
