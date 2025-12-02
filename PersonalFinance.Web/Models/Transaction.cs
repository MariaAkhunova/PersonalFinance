namespace PersonalFinance.Web.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime Date { get; set; }
        public int CategoryId { get; set; }
        public int UserId { get; set; }  // Здесь было UseId - исправляем на UserId
        public Category? Category { get; set; }
    }

    public class TransactionCreateModel
    {
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public int CategoryId { get; set; }
    }

    public class TransactionSummary
    {
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
        public decimal Balance { get; set; }
        public int TransactionCount { get; set; }
    }
}