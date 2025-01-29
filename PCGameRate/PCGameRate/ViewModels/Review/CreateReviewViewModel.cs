using System.ComponentModel.DataAnnotations;

namespace PCGameRate.ViewModels.Review
{
    public class CreateReviewViewModel
    {
        public int GameId { get; set; }
        [Required]
        [StringLength(1000, ErrorMessage = "Рецензія не може бути довшою за 1000 символів.")]
        public string Content { get; set; }
    }
}
