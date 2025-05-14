// ✅ Обновлённый RegisterViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace AttestationProject.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Почта")]
        public string Email { get; set; } = "";

        [Required]
        [StringLength(16, MinimumLength = 8, ErrorMessage = "Пароль должен содержать от 8 до 16 символов")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,16}$", ErrorMessage = "Пароль должен содержать буквы и цифры")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = "";

        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        public string ConfirmPassword { get; set; } = "";
    }
}
