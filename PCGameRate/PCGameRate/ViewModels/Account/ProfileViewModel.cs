using System.ComponentModel.DataAnnotations;

namespace PCGameRate.ViewModels.Account
{
    public class ProfileViewModel
    {
        [Display(Name = "Ваше ім'я")]
        public string FullName { get; set; }

        [Display(Name = "Аватар")]
        public string ProfileImageUrl { get; set; } 

        // Поле для завантаження файлу
        [Display(Name = "Завантажити аватар")]
        public IFormFile ProfileImage { get; set; }
    }
}
