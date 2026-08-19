namespace TravelManagement.Domain.Entities;

public class ExpenseCategory
{
    public int ExpenseCategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation Property
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}