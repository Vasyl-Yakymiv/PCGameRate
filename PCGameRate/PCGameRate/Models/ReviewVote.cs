using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PCGameRate.Models
{
    public class ReviewVote
    {
        public int VoteId { get; set; }
        public int ReviewId { get; set; }
        [ForeignKey("User")]
        public string Id { get; set; }
        public User User { get; set; }
        public bool IsLike { get; set; }
        public Review Review { get; set; }

    }
}
