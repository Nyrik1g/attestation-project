using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AttestationProject.Models
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(100)]
        public string? FullName { get; set; }

        [MaxLength(200)]
        public string? Address { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        public string? ProfileImagePath { get; set; }
        public string? ProfileImage { get; set; }
    }
}
