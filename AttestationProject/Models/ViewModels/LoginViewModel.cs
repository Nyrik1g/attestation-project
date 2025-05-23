﻿using System.ComponentModel.DataAnnotations;

namespace AttestationProject.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required, EmailAddress]
        [Display(Name = "Эл. почта")]
        public string Email { get; set; } = null!;

        [Required, DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = null!;

        [Display(Name = "Запомнить меня")]
        public bool RememberMe { get; set; }
    }
}
