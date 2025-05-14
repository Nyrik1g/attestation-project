using System.ComponentModel.DataAnnotations;

namespace AttestationProject.Models.ViewModels
{
    public class ProfileViewModel
    {
        [Display(Name = "Имя и Фамилия")]
        public string? FullName { get; set; }

        [Display(Name = "Адрес")]
        public string? Address { get; set; }

        [Display(Name = "Телефон")]
        [Phone]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Фото профиля")]
        public IFormFile? ProfileImage { get; set; }

        public string? ExistingImagePath { get; set; }

        public string? Email { get; set; }
    }
}
