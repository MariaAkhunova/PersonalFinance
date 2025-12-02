namespace PersonalFinance.Web.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsIncome { get; set; }
        public string? Description { get; set; }
        public int UserId { get; set; }
    }

    public class CategoryCreateModel
    {
        public string Name { get; set; } = string.Empty;
        public bool IsIncome { get; set; }
        public string? Description { get; set; }
    }
}