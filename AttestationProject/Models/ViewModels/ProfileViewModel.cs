using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace AttestationProject.Models.ViewModels
{
    public class ProfileViewModel
    {
        [Display(Name = "Электронная почта")]
        [EmailAddress]
        public string? Email { get; set; }

        [Display(Name = "ФИО")]
        public string? FullName { get; set; }

        [Display(Name = "Адрес")]
        public string? Address { get; set; }

        [Display(Name = "Телефон")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Аватар")]
        public IFormFile? ProfileImage { get; set; }

        // Для отображения уже загруженной картинки
        public string? ExistingImagePath { get; set; }
    }
}
