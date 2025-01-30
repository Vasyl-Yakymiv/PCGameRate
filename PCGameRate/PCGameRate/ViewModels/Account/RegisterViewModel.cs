using System.ComponentModel.DataAnnotations;

namespace PCGameRate.ViewModels.Account
{
    public class RegisterViewModel
    {
        [Display(Name = "Електронна пошта")]
        [Required(ErrorMessage = "Електронна пошта є обов'язковою")]
        public string EmailAddress { get; set; }
        [Required(ErrorMessage = "Пароль є обов'язковим")]
        [Display(Name = "Пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Display(Name = "Повторіть пароль")]
        [Required(ErrorMessage = "Повторне введення паролю є обов'язковим")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password do not match")]
        public string ConfirmPassword { get; set; }
    }
}
