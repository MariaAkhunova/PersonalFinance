using System.ComponentModel.DataAnnotations;

namespace PersonalFinance.API.Models.DTOs
{
    public class CategoryDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public bool IsIncome { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class CategoryResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsIncome { get; set; }
        public string? Description { get; set; }
        public int UserId { get; set; }
    }
}