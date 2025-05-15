using System.ComponentModel.DataAnnotations;

namespace AttestationProject.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required, EmailAddress]
        [Display(Name = "Эл. почта")]
        public string Email { get; set; } = null!;

        [Required, DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = null!;

        [Required, DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
        [Display(Name = "Повторите пароль")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
