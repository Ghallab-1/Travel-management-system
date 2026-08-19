namespace TravelManagement.Domain.Entities;

public class ExpenseReceipt
{
    public int ExpenseReceiptId { get; set; }

    // Foreign Key
    public int ExpenseId { get; set; }

    // Receipt Information
    public string FileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public string FileType { get; set; } = string.Empty;

    public DateTime UploadDate { get; set; }

    // Navigation Property
    public Expense Expense { get; set; } = null!;
}