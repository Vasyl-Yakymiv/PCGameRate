using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PCGameRate.Models
{
    public class GameListItem
    {
        [Key]
        public int GameListItemId { get; set; }
        [ForeignKey("User")]
        public string Id { get; set; }  
        public User User { get; set; }
        [ForeignKey("Game")]
        public int GameId { get; set; }  
        public Game Game { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.Now;
        
    }
}
