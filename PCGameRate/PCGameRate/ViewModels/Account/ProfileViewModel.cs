using System.ComponentModel.DataAnnotations;

namespace PCGameRate.ViewModels.Account
{
    public class ProfileViewModel
    {
        [Required]
        [Display(Name = "Повне ім'я")]
        public string FullName { get; set; }

        [Display(Name = "Зображення профілю")]
        public string ProfileImageUrl { get; set; }
    }
}
