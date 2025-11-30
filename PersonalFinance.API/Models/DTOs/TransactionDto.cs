using System.ComponentModel.DataAnnotations;

namespace PersonalFinance.API.Models.DTOs
{
    public class TransactionDto
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [Required]
        public int CategoryId { get; set; }
    }

    public class TransactionResponseDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime Date { get; set; }
        public int CategoryId { get; set; }
        public int UserId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public bool IsIncome { get; set; }
    }
}