using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttestationProject.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите название")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите описание")]
        public string Description { get; set; } = string.Empty;

        [Range(0.01, 100000, ErrorMessage = "Цена должна быть больше 0")]
        [Precision(18, 2)]
        public decimal Price { get; set; }

        public string? ImagePath { get; set; }

        [Required(ErrorMessage = "Выберите категорию")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Выберите подкатегорию")]
        public string Subcategory { get; set; } = string.Empty;

        public string CreatedBy { get; set; } = string.Empty;
    }
}
