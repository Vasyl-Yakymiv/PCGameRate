using System.ComponentModel.DataAnnotations;

namespace PCGameRate.ViewModels.Account
{
    public class LoginViewModel
    {
        [Display(Name = "Електронна пошта")]
        [Required(ErrorMessage = "Електронна пошта є обов'язковою")]
        public string EmailAddress { get; set; }
        [Display(Name = "Пароль")]
        [Required(ErrorMessage = "Пароль є обов'язковим")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
