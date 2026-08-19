namespace TravelManagement.Domain.Entities;

public class Currency
{
    public int CurrencyId { get; set; }

    public string CurrencyCode { get; set; } = string.Empty;

    public string CurrencyName { get; set; } = string.Empty;

    public decimal ExchangeRate { get; set; }

    public DateTime LastUpdated { get; set; }

    // Navigation Property
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}