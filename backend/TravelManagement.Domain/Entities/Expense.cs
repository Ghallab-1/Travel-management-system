namespace TravelManagement.Domain.Entities;

public class Expense
{
    public int ExpenseId { get; set; }

    // Foreign Keys
    public int TravelRequestId { get; set; }

    public int UserId { get; set; }

    public int ExpenseCategoryId { get; set; }

    public int CurrencyId { get; set; }

    // Expense Information
    public decimal Amount { get; set; }

    public string? Description { get; set; }

    public DateOnly ExpenseDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime SubmittedDate { get; set; }

    // Navigation Properties
    public TravelRequest TravelRequest { get; set; } = null!;

    public User User { get; set; } = null!;

    public ExpenseCategory ExpenseCategory { get; set; } = null!;

    public Currency Currency { get; set; } = null!;

    public ICollection<ExpenseReceipt> ExpenseReceipts { get; set; } = new List<ExpenseReceipt>();
}